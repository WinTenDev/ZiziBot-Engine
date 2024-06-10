﻿using Flurl;
using Humanizer;
using Octokit.Webhooks;
using Octokit.Webhooks.Events;
using Octokit.Webhooks.Events.PullRequest;
using Octokit.Webhooks.Events.Star;
using ZiziBot.Types.Vendor.GitHub;
using ZiziBot.Types.Vendor.GitLab;

namespace ZiziBot.Application.Services;

public class WebhookService
{
    public async Task<WebhookResponseBase<bool>> ParseGitHub(WebhookHeader header, string payload)
    {
        var htmlMessage = HtmlMessage.Empty;
        var response = new WebhookResponseBase<bool>();

        var githubEvent = payload.Deserialize<GitHubEventBase>();
        var action = githubEvent!.Action;
        var repository = githubEvent!.Repository;
        var sender = githubEvent.Sender;

        htmlMessage.Url($"{repository?.HtmlUrl}", $"🗼 {repository?.FullName}").Br();

        switch (header.Event)
        {
            case WebhookEventType.Push:
                var pushEvent = payload.Deserialize<PushEvent>();
                var commits = pushEvent!.Commits.ToList();
                var commitCount = commits.Count;
                var commitsStr = "commit".ToQuantity(commitCount);
                var branchName = githubEvent.Ref?.Split('/').Last();
                var treeUrl = repository?.HtmlUrl.AppendPathSegment($"tree/{branchName}");

                htmlMessage.Text("🚀 Push ").Url(pushEvent.Compare, $"{commitsStr}").Bold($" to ").Url(treeUrl, $"{branchName}")
                    .Br().Br();

                commits.ForEach(commit => {
                    htmlMessage.Url(commit.Url.ToString(), commit.Id[..7])
                        .Text(": ")
                        .TextBr($"{commit.Message} by {commit.Author.Name}");
                });
                break;

            case WebhookEventType.PullRequest:
                var pullRequestEvent = payload.Deserialize<PullRequestEvent>();
                var pullRequest = pullRequestEvent!.PullRequest;
                var headUrl = pullRequest.Head.Repo.HtmlUrl.AppendPathSegment($"tree/{pullRequest.Head.Ref}");
                var baseUrl = pullRequest.Base.Repo.HtmlUrl.AppendPathSegment($"tree/{pullRequest.Base.Ref}");

                htmlMessage.Bold(action == PullRequestAction.Opened ? "🔌 Opened " : "🔌 Updated ")
                    .Url(pullRequest.HtmlUrl, $"PR #{pullRequest.Number}").Text(": ")
                    .Text(pullRequest.Title).Br()
                    .Bold("🎯 ").Url(headUrl, pullRequest.Head.Ref).Bold(" -> ").Url(baseUrl, pullRequest.Base.Ref).Br();
                break;

            case WebhookEventType.Star:
                var watcherCount = repository.WatchersCount;

                htmlMessage.Bold(action == StarAction.Created ? "⭐️ Starred " : "🌟 Unstarred ")
                    .Url(repository.HtmlUrl, repository.FullName).Br()
                    .Bold("Total: ").Code(watcherCount.ToString()).Br();
                break;

            case WebhookEventType.Status:
                var statusEvent = payload.Deserialize<StatusEvent>();
                htmlMessage
                    .Bold("Creator: ").TextBr(sender.Login)
                    .Bold("Status: ").Url(statusEvent.TargetUrl, statusEvent.State.StringValue);
                break;

            case WebhookEventType.DeploymentStatus:
                var deploymentStatusEvent = payload.Deserialize<DeploymentStatusEvent>();
                htmlMessage
                    .Bold("Creator: ").TextBr(deploymentStatusEvent.Deployment.Creator.Login)
                    .Bold("Environment: ").TextBr(deploymentStatusEvent.DeploymentStatus.Environment).Br()
                    .Bold("Status: ").TextBr(deploymentStatusEvent.DeploymentStatus.State.StringValue);
                break;

            case WebhookEventType.Deployment:
                var deploymentEvent = payload.Deserialize<DeploymentEvent>();
                htmlMessage
                    .Bold("Creator: ").TextBr(deploymentEvent.Deployment.Creator.Login)
                    .Bold("Environment: ").TextBr(deploymentEvent.Deployment.Environment).Br()
                    .Bold("Status: ").TextBr(deploymentEvent.Deployment.Task);
                break;

            case WebhookEventType.WorkflowRun:
                var workflowRunEvent = payload.Deserialize<WorkflowRunEvent>();
                htmlMessage.Bold("Name: ").TextBr(workflowRunEvent.WorkflowRun.Name)
                    .Bold("Status: ").TextBr(workflowRunEvent.WorkflowRun.Status.StringValue)
                    .Bold("Actor: ").TextBr(workflowRunEvent.WorkflowRun.Actor.Login);
                break;

            case WebhookEventType.CheckSuite:
                var checkSuiteEvent = payload.Deserialize<CheckSuiteEvent>();
                htmlMessage.Bold("Name: ").TextBr(checkSuiteEvent.CheckSuite.App.Name)
                    .Bold("Status: ").TextBr(checkSuiteEvent.CheckSuite.Status.StringValue)
                    .Bold("Conclusion: ").TextBr(checkSuiteEvent.CheckSuite.Conclusion.StringValue);
                break;

            default:
                break;
        }


        await Task.Delay(0);

        response.FormattedHtml = htmlMessage.ToString();

        return response;
    }

    public async Task<WebhookResponseBase<bool>> ParseGitLab(WebhookHeader header, string payload)
    {
        var request = payload.Deserialize<GitLabEvent>();
        var response = new WebhookResponseBase<bool>();

        var eventName = request!.EventName;
        var repository = request.Repository;
        var project = request.Project;
        var branchName = request.Ref.Split('/').Last();
        var treeUrl = project.WebUrl.AppendPathSegment($"tree/{branchName}");

        var htmlMessage = HtmlMessage.Empty;

        switch (eventName)
        {
            case "push":
                var commits = request.Commits;
                var commitsStr = "commit".ToQuantity(commits.Count);

                htmlMessage
                    // .Url(pushEvent.Compare, $"🏗 {commitsStr}")
                    .Bold($"🏗 {commitsStr}")
                    .Bold($" to ").Url(project.WebUrl, $"{project.Name}")
                    .Text(":").Url(treeUrl, $"{branchName}")
                    .Br().Br();

                commits.ForEach(commit => {
                    htmlMessage.Url(commit.Url.ToString(), commit.Id[..7])
                        .Text(": ")
                        .TextBr($"{commit.Message.Trim()} by {commit.Author.Name}");
                });
                break;
        }

        await Task.Delay(1);

        response.FormattedHtml = htmlMessage.ToString();

        return response;
    }
}