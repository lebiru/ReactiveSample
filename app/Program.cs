using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace ReactiveWebApiExample
{
    class Program
    {
        static void Main(string[] args)
        {
            var interval = TimeSpan.FromSeconds(5);
            var logFilePath = "./url-scraper.txt"; // for Windows, use direct path
            
            // endpoints to scrape URLs from
            var endpoints = new[] 
            { 
                "https://www.example.com",
                "https://www.lipsum.com/feed/html" 
            };
            
            // define the fileWatcher for url-scraper.txt file
            var fileWatcher = new FileSystemWatcher
            {
                Path = Path.GetDirectoryName(logFilePath),
                Filter = Path.GetFileName(logFilePath),
                NotifyFilter = NotifyFilters.LastWrite
            };

            // A Subject can be thought of as a sort of "proxy observer" that sits 
            // between the observer and the data source. The observer subscribes to 
            // the Subject, and the Subject in turn subscribes to the data source. 
            // When the data source produces a new value, it passes that value to 
            // the Subject, which then passes it on to the observer.
            var endpointSubject = new Subject<string>();
            var fileSubject = new Subject<string>();

            // multiplex endpoint, able to call an enumerable list of endpoint strings and filter out for urls
            var endpointObservable = endpoints.ToObservable()
                .SelectMany(endpoint => Observable.Interval(interval).Select(_ => endpoint))
                .SelectMany(endpoint => GetEndpointDataAsync(endpoint))
                .Retry(2)
                .SelectMany(data => data.Split(new[] { Environment.NewLine }, StringSplitOptions.None))
                .Where(line => line.Contains("https://www"))
                .Select(line => line.Substring(line.IndexOf("https://www")))
                .Subscribe(endpointSubject);

            // define the fileObservable event handlers
             var fileObservable = Observable.FromEventPattern<FileSystemEventHandler, FileSystemEventArgs>(
                h => fileWatcher.Changed += h,
                h => fileWatcher.Changed -= h
            )
            .SelectMany(_ => ReadFileAsync(logFilePath))
            .Subscribe(fileSubject);

            // Subscriber/Consumer action for the endpoints
            // 1. Console Write what url data was received
            // 2. Append the url to the url-scraper.txt log file
            // 3. Console Write data was logged to file
            endpointSubject.Subscribe(data =>
            {
                var timenow = DateTime.Now.ToLongTimeString();
                Console.WriteLine($"[{timenow}] Data received from endpoint: {data}");
                File.AppendAllText(logFilePath, timenow + " " + data + Environment.NewLine);
                Console.WriteLine($"Data logged to file");
            });

            // Subscriber/Consumer action for the file watcher
            // 1. On File Write, subscribe to these actions
            // 2. Console Write the contents of the file
            fileSubject.Subscribe(data =>
            {
                var timenow = DateTime.Now.ToLongTimeString();
                Console.WriteLine($"[{timenow}] Total Data contents from file: {data}");
            });

            // Enable the File Watcher
            fileWatcher.EnableRaisingEvents = true;
            
            // Exit App Condition
            Console.WriteLine("Press any key to stop observing the endpoint and file");
            Console.ReadKey();
        }

        private static async Task<string> GetEndpointDataAsync(string endpoint)
        {
            using var httpClient = new HttpClient();
            return await httpClient.GetStringAsync(endpoint);
        }

        private static async Task<string> ReadFileAsync(string filePath)
        {
            return await File.ReadAllTextAsync(filePath);
        }
    }
}