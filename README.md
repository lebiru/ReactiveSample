# ReactiveSample

This is a sample dotnet console app that queries 2 endpoints and saves url strings from the page response.

The project leverages C# Reactive libraries and techniques to setup observers and subscribers for the processing.

1. Endpoint Observer

The endpoint observer calls all endpoints defined in the array of endpoint strings. The call attempts at most 2 times to get a proper response.
Then it splits the HTML response from the server by newlines.
Then it filters only lines that contain "https://www"
Then it does a substring of the url.

2. Endpoint Subject

The endpoint subject is passed to the Subscribe method of the Observer.

A Subject can be thought of as a sort of "proxy observer" that sits between the observer and the data source. The observer subscribes to the Subject, and the Subject in turn subscribes to the data source. When the data source produces a new value, it passes that value to the Subject, which then passes it on to the observer.

There are several different types of Subject in the System.Reactive namespace, each with slightly different behavior. For example, the ReplaySubject will cache the last n values and replay them to new subscribers, while the BehaviorSubject will always send the most recent value to new subscribers.

By using Subjects, you can manipulate the stream of data in various ways before it reaches the observer, such as filtering, transforming, and aggregating the data. This can be useful for implementing complex business logic or data pipelines that would be difficult to accomplish using traditional callback-based programming.

3. File Observable

This observable is called whenever changes to the file happened.
Using a fluent method call structure, we define that part of this observable is to ReadFileAsync of url-scraper.txt and pass the result 
to the subscriber file Subject.

4. File Subject

The file subject is used to subscribe to actions whenever the url-scraper.txt file changes. In this case I just output the contents of the file through the console, but it shows how easy you can set up a convery-belt style of processes to run based on an Observer/Subscriber pattern.


## How To Run

1. git clone the repo

2. dotnet build app

3. dotnet run --project app
