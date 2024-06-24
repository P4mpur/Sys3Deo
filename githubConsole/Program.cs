using System;
using System.Linq;
using System.Net;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

class Program
{
    static void Main(string[] args)
    {
         string token = Environment.GetEnvironmentVariable("GITHUB_TOKEN");

        if (token == null)
        {
            Console.WriteLine("Token je prazan");
            return;
        }

        var gitHubService = new GitHubService(token);
        var sentimentService = new SentimentService();

        HttpListener listener = new HttpListener();
        listener.Prefixes.Add("http://localhost:8080/");
        listener.Start();
        Console.WriteLine("Listening for HTTP requests at http://localhost:8080/");

        var contextObservable = Observable.Defer(() => Observable.FromAsync(listener.GetContextAsync)).Repeat();

        var subscription = contextObservable
            .SelectMany(context => Observable.FromAsync(() => HandleRequest(context, gitHubService, sentimentService)))
            .Subscribe();

        Console.WriteLine("Press Enter to stop...");
        Console.ReadLine();
        subscription.Dispose();
        listener.Stop();
    }

    private static async Task HandleRequest(HttpListenerContext context, GitHubService gitHubService, SentimentService sentimentService)
    {
        var request = context.Request;
        var response = context.Response;

        if (request.HttpMethod == "GET" && request.Url.AbsolutePath == "/analyze")
        {
            var query = HttpUtility.ParseQueryString(request.Url.Query);
            string owner = query["owner"];
            string repository = query["repository"];
            string issueNumberStr = query["issueNumber"];

            if (string.IsNullOrWhiteSpace(owner) || string.IsNullOrWhiteSpace(repository) || !int.TryParse(issueNumberStr, out int issueNumber))
            {
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                byte[] errorBuffer = Encoding.UTF8.GetBytes("Invalid query parameters");
                await response.OutputStream.WriteAsync(errorBuffer, 0, errorBuffer.Length);
                response.OutputStream.Close();
                return;
            }

            var comments = await gitHubService.GetIssueCommentsAsync(owner, repository, issueNumber);

            var results = comments.Select(comment => new
            {
                Comment = comment.Body,
                Sentiment = sentimentService.AnalyzeSentiment(comment.Body)
            }).ToList();

            // Calculate total sentiment scores
            var totalSentiment = new SentimentScore();
            foreach (var result in results)
            {
                totalSentiment.Compound += result.Sentiment.Compound;
                totalSentiment.Positive += result.Sentiment.Positive;
                totalSentiment.Neutral += result.Sentiment.Neutral;
                totalSentiment.Negative += result.Sentiment.Negative;
            }

            StringBuilder htmlResponse = new StringBuilder();
            htmlResponse.Append("<html><head><title>Sentiment Analysis Results</title>");
            

            htmlResponse.Append("body { font-family: Arial, sans-serif; margin: 40px; }");
            htmlResponse.Append("table { width: 100%; border-collapse: collapse; }");
            htmlResponse.Append("th, td { border: 1px solid #ddd; padding: 8px; text-align: left; max-width: 200px; overflow: hidden; white-space: nowrap; text-overflow: ellipsis; }");
            htmlResponse.Append("th { background-color: #f2f2f2; }");
            htmlResponse.Append("tr:nth-child(even) { background-color: #f9f9f9; }");
            htmlResponse.Append("tr:hover { background-color: #f1f1f1; }");
            htmlResponse.Append(".table-container { overflow-x: auto; }");
            htmlResponse.Append("</style>");
            htmlResponse.Append("</head><body>");
            htmlResponse.Append("<h1>Sentiment Analysis Results</h1>");
            htmlResponse.Append("<div class='table-container'><table><tr><th>Comment</th><th>Compound</th><th>Positive</th><th>Neutral</th><th>Negative</th></tr>");

                foreach (var result in results)
            {
            {
                htmlResponse.AppendFormat("<td title='{0}'>{1}</td>", WebUtility.HtmlEncode(result.Comment), WebUtility.HtmlEncode(TrimText(result.Comment, 200)));
                htmlResponse.AppendFormat("<td>{0}</td>", result.Sentiment.Compound);
                htmlResponse.AppendFormat("<td>{0}</td>", result.Sentiment.Positive);
                htmlResponse.AppendFormat("<td>{0}</td>", result.Sentiment.Neutral);
                htmlResponse.AppendFormat("<td>{0}</td>", result.Sentiment.Negative);
                htmlResponse.Append("</tr>");
                
}


                // Total summary
            // Total summary
            htmlResponse.Append("<table>");
            htmlResponse.Append("<tr><th>Total</th><th>Compound</th><th>Positive</th><th>Neutral</th><th>Negative</th></tr>");
            htmlResponse.Append("<tr>");
            htmlResponse.Append($"<td>Total</td>");
            htmlResponse.AppendFormat("<td>{0}</td>", totalSentiment.Compound);
            htmlResponse.AppendFormat("<td>{0}</td>", totalSentiment.Positive);
            htmlResponse.AppendFormat("<td>{0}</td>", totalSentiment.Neutral);
            htmlResponse.AppendFormat("<td>{0}</td>", totalSentiment.Negative);
            htmlResponse.Append("</tr>");
            htmlResponse.Append("</table>");

                htmlResponse.Append("</body></html>");

                // Helper method to trim text with ellipsis
            // Helper method to trim text with ellipsis
            {
            {
                    return text.Substring(0, maxLength - 3) + "...";
                    
                else
            }


            byte[] htmlBuffer = Encoding.UTF8.GetBytes(htmlResponse.ToString());

            response.ContentType = "text/html";
            response.ContentLength64 = htmlBuffer.Length;
            await response.OutputStream.WriteAsync(htmlBuffer, 0, htmlBuffer.Length);
            response.OutputStream.Close();
        }
        else
        {
            response.StatusCode = (int)HttpStatusCode.BadRequest;
            byte[] errorBuffer = Encoding.UTF8.GetBytes("Invalid request");
            await response.OutputStream.WriteAsync(errorBuffer, 0, errorBuffer.Length);
            response.OutputStream.Close();
        }
    }

    // Sample class representing sentiment score
    class SentimentScore
    {
        public double Compound { get; set; }
        public double Positive { get; set; }
        public double Neutral { get; set; }
        public double Negative { get; set; }
    }
}
