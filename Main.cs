using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using HtmlAgilityPack; 


namespace BrokenLinkChecker
{
    public partial class Main : Form
    {
        private ConcurrentQueue<string> _urlQueue = new ConcurrentQueue<string>();
        private ConcurrentBag<ErrorPage> _errorPages = new ConcurrentBag<ErrorPage>();
        private SemaphoreSlim _semaphore = new SemaphoreSlim(10); // Limit concurrent requests
        private CancellationTokenSource _cts = new CancellationTokenSource();
        private HashSet<string> _visitedUrls = new HashSet<string>(); // Track visited URLs
        private Uri _startUri;
        private string _domain;
        private int _maxPages = 10000;
        private int _visitedCount = 0;
        public Main()
        {
            InitializeComponent();
            dataGridViewErrors.AutoGenerateColumns = true; 
        }

        private void Main_Load(object sender, EventArgs e)
        {
            progressBarCrawling.Maximum = _maxPages;

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
        private void ResetState()
        {
            _urlQueue = new ConcurrentQueue<string>();
            _errorPages = new ConcurrentBag<ErrorPage>();
            _visitedUrls = new HashSet<string>();
            _visitedCount = 0;
            _cts = new CancellationTokenSource();
        }

        private async Task CrawlAsync(CancellationToken cancellationToken)
        {
            while (!_urlQueue.IsEmpty && _visitedCount < _maxPages && !cancellationToken.IsCancellationRequested)
            {
                if (!_semaphore.Wait(100)) continue;

                if (!_urlQueue.TryDequeue(out string url))
                {
                    _semaphore.Release();
                    continue;
                }

                if (!_visitedUrls.Add(url))
                {
                    _semaphore.Release();
                    continue;
                }

                try
                {
                    _visitedCount++;
                    UpdateProgressBar();

                    using (HttpClient client = new HttpClient())
                    {
                        HttpResponseMessage response = await client.GetAsync(url, cancellationToken);
                        response.EnsureSuccessStatusCode();

                        string content = await response.Content.ReadAsStringAsync();
                        var doc = new HtmlAgilityPack.HtmlDocument();
                        doc.LoadHtml(content);

                        await ProcessResourcesAsync(url, doc, client, cancellationToken);
                        EnqueueLinks(url, doc);
                    }
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

            SaveErrorsToCSV();
            Invoke((MethodInvoker)(() => Check_Button.Enabled = true));
        }

        private async Task ProcessResourcesAsync(string url, HtmlAgilityPack.HtmlDocument doc, HttpClient client, CancellationToken cancellationToken)
        {
            var resourceLinks = doc.DocumentNode.SelectNodes("//link[@rel='stylesheet' or @src]")
                ?.Select(node => node.GetAttributeValue("href", node.GetAttributeValue("src", null)))
                .Where(link => !string.IsNullOrEmpty(link))
                .Distinct();

            if (resourceLinks != null)
            {
                foreach (string link in resourceLinks)
                {
                    string fullUrl = new Uri(new Uri(url), link).ToString();
                    if (IsSameDomain(fullUrl))
                    {
                        int? errorCode = await CheckResourceAsync(fullUrl, client, cancellationToken);
                        if (errorCode.HasValue && errorCode.Value >= 400)
                        {
                            _errorPages.Add(new ErrorPage
                            {
                                PageURL = url,
                                ResourceURL = fullUrl,
                                ErrorCode = errorCode.Value
                            });
                            UpdateDataGridView();
                        }
                    }
                }
            }
        }

        private void EnqueueLinks(string url, HtmlAgilityPack.HtmlDocument doc)
        {
            var links = doc.DocumentNode.SelectNodes("//a[@href]")
                ?.Select(node => node.GetAttributeValue("href", ""))
                .Where(link => !string.IsNullOrEmpty(link))
                .Select(link => new Uri(new Uri(url), link).ToString())
                .Where(link => IsSameDomain(link) && !_visitedUrls.Contains(link));

            if (links != null)
            {
                foreach (string link in links)
                {
                    _urlQueue.Enqueue(link);
                }
            }
        }

        private async Task<int?> CheckResourceAsync(string url, HttpClient client, CancellationToken cancellationToken)
        {
            try
            {
                HttpResponseMessage response = await client.SendAsync(new HttpRequestMessage(HttpMethod.Head, url), cancellationToken);
                response.EnsureSuccessStatusCode();  // Throws an exception if the status code is not successful
                return (int?)response.StatusCode;
            }
            catch (HttpRequestException ex)
            {
                // Handle HttpRequestException (e.g., no network connection, invalid URL, etc.)
                // In this case, we can return the status code from the exception message
                return null; // You can customize this as needed (e.g., logging or returning a specific error code)
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

        private void UpdateProgressBar()
        {
            progressBarCrawling.Invoke((MethodInvoker)(() => progressBarCrawling.Value = Math.Min(_visitedCount, _maxPages)));
        }

        private void UpdateDataGridView()
        {
            if (dataGridViewErrors.InvokeRequired)
            {
                dataGridViewErrors.Invoke((MethodInvoker)UpdateDataGridView);
                return;
            }

            dataGridViewErrors.DataSource = _errorPages.ToList();
        }

        private void SaveErrorsToCSV()
        {
            string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "error_details.csv");

            using (StreamWriter writer = new StreamWriter(filePath, false, Encoding.UTF8))
            {
                writer.WriteLine("PageURL,ResourceURL,ErrorCode");
                foreach (var error in _errorPages)
                {
                    writer.WriteLine($"{error.PageURL},{error.ResourceURL},{error.ErrorCode}");
                }
            }

            MessageBox.Show($"Error details saved to {filePath}");
        }

        private bool IsSameDomain(string url)
        {
            try
            {
                return new Uri(url).Host == _domain;
            }
            catch
            {
                return false;
            }
        }
    }
 
}
