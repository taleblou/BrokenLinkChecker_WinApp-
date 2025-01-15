# **Web Crawler** 

## **Overview**

This project is a Web Crawler application built with C\# and Windows Forms. The crawler visits web pages, extracts resources (CSS, JavaScript, images, videos, iframes), checks if these resources are available (status code 200), and logs any errors encountered (e.g., 404 or 500 errors). The application also collects all internal links from the crawled pages and continues to explore them recursively, up to a specified limit of pages.

## **Features**

* Start crawling from a provided URL and explore links within the same domain.  
* Extract and check resources (CSS, JS, images, etc.) linked in the crawled pages.  
* Display errors (e.g., 404 or 500 errors) in a data grid.  
* Save error details to a CSV file.  
* Limit the number of pages crawled and the number of concurrent requests.  
* Support for canceling the crawl operation.  
* Track the status of the crawl and show progress.

## **Prerequisites**

* .NET Framework (Windows Forms application).  
* `HtmlAgilityPack` package for HTML parsing.  
* `DataFrames` for handling CSV saving.

## **How to Run the Application**

1. **Clone or Download** the source code to your local machine.  
2. **Install Dependencies**:

Install the `HtmlAgilityPack` via NuGet:  
bash  
Copy code  
`Install-Package HtmlAgilityPack`

*   
  * Ensure the `DataFrames` package is referenced in the project.  
3. **Build and Run** the application in Visual Studio.

## **Usage**

1. **Enter the Starting URL** in the text box provided.  
2. Click the **Start** button to begin crawling.  
3. The application will start crawling the pages within the domain, checking for resources like CSS, JavaScript, images, etc.  
4. Any pages that result in errors (e.g., 404, 500\) will be logged and displayed in the data grid.  
5. You can **cancel** the crawl at any time by closing the form or stopping the process via the application.

## **Main Components**

### **1\. `MainForm`**

* Controls the main user interface for the crawler.  
* Handles UI interaction, such as starting the crawl and updating progress.

### **2\. `CrawlAsync`**

* Executes the crawling operation asynchronously.  
* Manages the queue of URLs to visit and checks for errors in resources.  
* Limits the number of concurrent HTTP requests using a semaphore.

### **3\. `IsSameDomain`**

* Ensures that only links within the same domain as the starting URL are crawled.

### **4\. `CheckResourceAsync`**

* Sends HEAD requests to resources to verify their availability.

### **5\. `SaveErrorsToCSV`**

* Saves error details (URL and status code) to a CSV file for further analysis.

### **6\. `ErrorPage`**

* A model class that holds information about an error encountered (page URL, resource URL, and error code).

## **Error Handling**

* If an error is encountered while crawling a page or checking a resource, the error is logged in the `ErrorPage` list.  
* The status code of the error is captured, and the relevant URLs are displayed in the UI.  
* A CSV file named `error_details.csv` is generated at the end of the crawling process with all logged errors.

## **Configuration**

* **Max Pages**: The crawler is limited to visiting a maximum of 10,000 pages by default. This can be adjusted in the code.  
* **Concurrent Requests**: The number of concurrent HTTP requests is limited to 10 via a semaphore.

## **Notes**

* The `HttpClient` is used to send requests to the URLs and check the availability of resources.  
* The application uses `HtmlAgilityPack` to parse and manipulate the HTML of the crawled pages.  
* The program is designed to be stopped safely by canceling the `CancellationToken`.

## **Future Enhancements**

* Implement additional functionality to handle deeper crawling levels, such as crawling external links or adding a delay between requests.  
* Allow the user to specify the maximum number of errors before stopping the crawl.  
* Improve the user interface with more detailed progress indicators.

## **License**

This project is licensed under the MIT License \- see the LICENSE file for details.

