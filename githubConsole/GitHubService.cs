using Octokit;
using System.Collections.Generic;
using System.Threading.Tasks;

public class GitHubService
{
    private readonly GitHubClient _client;

    public GitHubService(string token)
    {
        _client = new GitHubClient(new ProductHeaderValue("GitHubSentimentAnalysis"))
        {
            Credentials = new Credentials(token)
        };
    }

    // mozda ne treba ovde public async jer imamo task
    public async Task<IReadOnlyList<IssueComment>> GetIssueCommentsAsync(string owner, string repo, int issueNumber)
    {
        return await _client.Issue.Comment.GetAllForIssue(owner, repo, issueNumber);
    }
}
