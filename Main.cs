using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms; 
using System.ComponentModel;

namespace BrokenLinkChecker
{
    public partial class Main : Form
    {
        private ConcurrentQueue<string> _urlQueue = new ConcurrentQueue<string>();
        private ConcurrentBag<ErrorPage> _errorPages = new ConcurrentBag<ErrorPage>();
        private SemaphoreSlim _semaphore = new SemaphoreSlim(10); // Limit concurrent requests
        private CancellationTokenSource _cts = new CancellationTokenSource();
        private Uri _startUri;
        private string _domain;
        private int _maxPages = 10000;
        private int _visitedCount = 0;
        public Main()
        {
            InitializeComponent();
            dataGridViewErrors.AutoGenerateColumns = true;
            this.FormClosing += MainForm_FormClosing;
        }

        private void Main_Load(object sender, EventArgs e)
        {

        }

        private void Check_Button_Click(object sender, EventArgs e)
        {
            string startUrl = URL_TextBox.Text.Trim();
            if (string.IsNullOrEmpty(startUrl))
            {
                MessageBox.Show("Please enter a starting URL.");
                return;
            }

            try
            {
                _startUri = new Uri(startUrl);
                _domain = _startUri.Host;
            }
            catch (UriFormatException ex)
            {
                MessageBox.Show($"Invalid URL: {ex.Message}");
                return;
            }

            _urlQueue = new ConcurrentQueue<string>();
            _errorPages = new ConcurrentBag<ErrorPage>();
            _visitedCount = 0;
            _cts = new CancellationTokenSource();

            _urlQueue.Enqueue(_startUri.ToString());
            progressBarCrawling.Value = 0;
            //lblStatus.Text = "Crawling in progress...";
            Check_Button.Enabled = false;

            Task.Run(() => CrawlAsync(_cts.Token));
        }
        private async Task CrawlAsync(CancellationToken cancellationToken)
        {
            while (!_urlQueue.IsEmpty && _visitedCount < _maxPages && !cancellationToken.IsCancellationRequested)
            {
                if (!_semaphore.Wait(100)) continue; // Wait if all slots are busy

                string url;
                if (!_urlQueue.TryDequeue(out url))
                {
                    _semaphore.Release();
                    continue;
                }

                if (_visitedCount >= _maxPages)
                {
                    _semaphore.Release();
                    break;
                }

                if (cancellationToken.IsCancellationRequested)
                {
                    _semaphore.Release();
                    break;
                }

                if (url.Contains(_domain))
                {
                    try
                    {
                        _visitedCount++;
                        progressBarCrawling.Invoke((MethodInvoker)(() =>
                        {
                            progressBarCrawling.Value = _visitedCount;
                        }));

                        using (HttpClient client = new HttpClient())
                        {
                            HttpResponseMessage response = await client.GetAsync(url, cancellationToken);
                            response.EnsureSuccessStatusCode();

                            string content = await response.Content.ReadAsStringAsync();

                            HtmlDocument doc = new HtmlDocument();
                            doc.LoadHtml(content);

                            // Collect all resource links
                            var resourceLinks = new List<string>();
                            // CSS links
                            resourceLinks.AddRange(doc.DocumentNode.SelectNodes("//link[@rel='stylesheet']/@href")
                                .Select(node => node.Value));
                            // JavaScript links
                            resourceLinks.AddRange(doc.DocumentNode.SelectNodes("//script[@src]/@src")
                                .Select(node => node.Value));
                            // Image links
                            resourceLinks.AddRange(doc.DocumentNode.SelectNodes("//img[@src]/@src")
                                .Select(node => node.Value));
                            // Video links
                            resourceLinks.AddRange(doc.DocumentNode.SelectNodes("//video[@src]/@src")
                                .Select(node => node.Value));
                            resourceLinks.AddRange(doc.DocumentNode.SelectNodes("//video/source[@src]/@src")
                                .Select(node => node.Value));
                            // Iframe links
                            resourceLinks.AddRange(doc.DocumentNode.SelectNodes("//iframe[@src]/@src")
                                .Select(node => node.Value));

                            // Check each resource
                            foreach (string link in resourceLinks)
                            {
                                string fullUrl = new Uri(new Uri(url), link).ToString();
                                if (IsSameDomain(fullUrl))
                                {
                                    int? error = await CheckResourceAsync(fullUrl, client, cancellationToken);
                                    if (error.HasValue && error.Value >= 400 && error.Value < 600)
                                    {
                                        _errorPages.Add(new ErrorPage
                                        {
                                            PageURL = url,
                                            ResourceURL = fullUrl,
                                            ErrorCode = error.Value
                                        });
                                        UpdateDataGridView();
                                    }
                                }
                            }

                            // Find all links in the page and add to queue
                            var links = doc.DocumentNode.SelectNodes("//a[@href]")
                                .Select(node => node.GetAttributeValue("href", ""))
                                .Select(link => new Uri(new Uri(url), link).ToString())
                                .Where(link => IsSameDomain(link) && !_urlQueue.Contains(link) && !_visitedUrls.Contains(link));

                            foreach (string link in links)
                            {
                                _urlQueue.Enqueue(link);
                            }
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        // Ignore if canceled
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error crawling {url}: {ex.Message}");
                    }
                    finally
                    {
                        _semaphore.Release();
                    }
                }
                else
                {
                    _semaphore.Release();
                    continue;
                }
            }

            // Save error details to CSV
            SaveErrorsToCSV();

            Invoke((MethodInvoker)(() =>
            {
                //lblStatus.Text = "Crawling completed.";
                Check_Button.Enabled = true;
            }));
        }

        private bool IsSameDomain(string url)
        {
            try
            {
                Uri uri = new Uri(url);
                return uri.Host == _domain;
            }
            catch (UriFormatException)
            {
                return false;
            }
        }

        private async Task<int?> CheckResourceAsync(string url, HttpClient client, CancellationToken cancellationToken)
        {
            try
            {
                HttpResponseMessage response = await client.SendAsync(new HttpRequestMessage(HttpMethod.Head, url), cancellationToken);
                response.EnsureSuccessStatusCode();
                return (int?)response.StatusCode;
            }
            catch (HttpRequestException ex)
            {
                return (int?)ex.StatusCode;
            }
            catch (TaskCanceledException)
            {
                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private void UpdateDataGridView()
        {
            if (dataGridViewErrors.InvokeRequired)
            {
                dataGridViewErrors.Invoke((MethodInvoker)(() => UpdateDataGridView()));
                return;
            }

            List<ErrorPage> errors = _errorPages.ToList();
            dataGridViewErrors.DataSource = errors;
        }

        private void SaveErrorsToCSV()
        {
            List<ErrorPage> errors = _errorPages.ToList();
            if (errors.Any())
            {
                DataFrame df = DataFrame.FromRecords(errors);
                df.ToCSV("error_details.csv");
            }
            else
            {
                MessageBox.Show("No error pages to save.");
            }
        }
    }
}
