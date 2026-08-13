using Flurl;
using Humanizer;
using Octokit.Webhooks;
using Octokit.Webhooks.Events;
using Octokit.Webhooks.Events.PullRequest;
using Octokit.Webhooks.Events.Star;
using ZiziBot.Application.Common.Types;
using ZiziBot.Application.Infrastructure.Vendor.GitHub;
using ZiziBot.Application.Infrastructure.Vendor.GitLab;

namespace ZiziBot.Application.Services;

public class WebhookService
{
    public async Task<WebhookResponseBase<bool>> ParseGitHub(WebhookHeader? header, string payload)
    {
        var htmlMessage = HtmlMessage.Empty;
        var response = new WebhookResponseBase<bool>();

        var githubEvent = payload.Deserialize<GitHubEventBase>();
        var action = githubEvent!.Action;
        var repository = githubEvent.Repository;
        var sender = githubEvent.Sender;

        if (repository != null)
            htmlMessage.Url($"{repository?.HtmlUrl}", $"🗼 {repository?.FullName}").Br();

        switch (header.Event)
        {
            case WebhookEventType.Ping:
                var pingEvent = payload.Deserialize<PingEvent>();
                var hook = pingEvent.Hook;

                htmlMessage.Bold("Webhook installed").Br()
                    .Bold("Type: ").Code(hook.Type.StringValue);
                break;
            case WebhookEventType.Push:
                var pushEvent = payload.Deserialize<PushEvent>();
                var commits = pushEvent!.Commits.ToList();
                var commitCount = commits.Count;
                var commitsStr = "commit".ToQuantity(commitCount);
                var branchName = githubEvent.Ref?.Split('/').Last();
                var treeUrl = repository?.HtmlUrl.AppendPathSegment($"tree/{branchName}");

                htmlMessage.Text("🚀 Push ").Url(pushEvent.Compare, $"{commitsStr}").Bold($" to ").Url(treeUrl, $"{branchName}")
                    .Br().Br();

                commits.ForEach(commit =>
                {
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

                htmlMessage
                    .Bold(action == PullRequestAction.Opened ? "🔌 Opened " : "🔌 Updated ")
                    .Url(pullRequest.HtmlUrl, $"PR #{pullRequest.Number}").Text(": ")
                    .Text(pullRequest.Title).Br()
                    .Bold("🎯 ").Url(headUrl, pullRequest.Head.Ref).Bold(" -> ").Url(baseUrl, pullRequest.Base.Ref).Br();
                break;

            case WebhookEventType.Star:
                var watcherCount = repository.WatchersCount;
                var watchEvent = payload.Deserialize<StarEvent>();

                htmlMessage
                    .Bold(action == StarAction.Created ? "⭐️ Starred " : "🌟 Unstarred ").Code(watcherCount.ToString()).Br();
                break;

            case WebhookEventType.Status:
                var statusEvent = payload.Deserialize<StatusEvent>();

                htmlMessage
                    .Bold("Creator: ").TextBr(sender.Login)
                    .Bold("Status: ").Url(statusEvent.TargetUrl, statusEvent.State.StringValue);
                break;

            case WebhookEventType.DeploymentStatus:
                var deploymentStatusEvent = payload.Deserialize<DeploymentStatusEvent>();
                var deploymentStatus = deploymentStatusEvent!.DeploymentStatus;

                htmlMessage
                    .Bold("Creator: ").TextBr(deploymentStatus.Creator.Login)
                    .Bold("Environment: ").TextBr(deploymentStatus.Environment)
                    .Bold("Status: ").TextBr(deploymentStatus.State.StringValue);
                break;

            case WebhookEventType.Deployment:
                var deploymentEvent = payload.Deserialize<DeploymentEvent>();
                var deployment = deploymentEvent!.Deployment;

                htmlMessage
                    .Bold("Creator: ").TextBr(deployment.Creator.Login)
                    .Bold("Environment: ").TextBr(deployment.Environment)
                    .Bold("Status: ").TextBr(deployment.Task);
                break;

            case WebhookEventType.WorkflowRun:
                var workflowRunEvent = payload.Deserialize<WorkflowRunEvent>();
                var workflowRun = workflowRunEvent.WorkflowRun;

                htmlMessage
                    .Bold("Name: ").TextBr(workflowRun.Name)
                    .Bold("Status: ").TextBr(workflowRun.Status.StringValue)
                    .Bold("Actor: ").TextBr(workflowRun.Actor.Login);
                break;

            case WebhookEventType.WorkflowJob:
                var workflowJobEvent = payload.Deserialize<WorkflowJobEvent>();
                var workflowJob = workflowJobEvent.WorkflowJob;

                htmlMessage
                    .Bold("Name: ").TextBr(workflowJob.Name)
                    .Bold("Workflow Name: ").TextBr(workflowJob.WorkflowName)
                    .Bold("Status: ").TextBr(workflowJob.Status.StringValue);
                break;

            case WebhookEventType.CheckSuite:
                var checkSuiteEvent = payload.Deserialize<CheckSuiteEvent>();
                var checkSuite = checkSuiteEvent!.CheckSuite;

                htmlMessage
                    .Bold("Name: ").TextBr(checkSuite.App.Name)
                    .Bold("Status: ").TextBr(checkSuite.Status?.StringValue)
                    .Bold("Conclusion: ").TextBr(checkSuite.Conclusion?.StringValue);
                break;

            case WebhookEventType.CheckRun:
                var checkRunEvent = payload.Deserialize<CheckRunEvent>();
                var checkRun = checkRunEvent!.CheckRun;

                htmlMessage
                    .Bold("Name: ").TextBr(checkRun.App.Name)
                    .Bold("Status: ").TextBr(checkRun.Status.StringValue);

                if (checkRun.Conclusion != null)
                    htmlMessage.Bold("Conclusion: ").TextBr(checkRun.Conclusion?.StringValue);
                break;

            case WebhookEventType.DependabotAlert:
                var dependabotAlertEvent = payload.Deserialize<DependabotAlertEvent>();
                var dependabotAlert = dependabotAlertEvent!.Alert;

                htmlMessage
                    .Bold("Status: ").CodeBr(dependabotAlert.State.StringValue)
                    .Bold("CVE ID: ").CodeBr(dependabotAlert.SecurityAdvisory.CveId)
                    .Bold("Summary: ").CodeBr(dependabotAlert.SecurityAdvisory.Summary)
                    .Bold("Severity: ").CodeBr(dependabotAlert.SecurityAdvisory.Severity.StringValue)
                    .Url(dependabotAlert.HtmlUrl, "Open details");
                break;

            case WebhookEventType.RepositoryVulnerabilityAlert:
                var repositoryVulnerabilityAlertEvent = payload.Deserialize<RepositoryVulnerabilityAlertEvent>();
                var repositoryVulnerabilityAlert = repositoryVulnerabilityAlertEvent!.Alert;

                htmlMessage
                    .Bold("CVE ID: ").CodeBr(repositoryVulnerabilityAlert.ExternalIdentifier)
                    .Bold("Package Name: ").CodeBr(repositoryVulnerabilityAlert.AffectedPackageName)
                    .Bold("Affected Range: ").CodeBr(repositoryVulnerabilityAlert.AffectedRange)
                    .Bold("Fixed In: ").CodeBr(repositoryVulnerabilityAlert.FixedIn)
                    .Url(repositoryVulnerabilityAlert.ExternalReference, "Open details");
                break;

            default:
                break;
        }


        await Task.Delay(0);

        response.FormattedHtml = htmlMessage.ToString();

        return response;
    }

    public async Task<WebhookResponseBase<bool>> ParseGitLab(WebhookHeader? header, string payload)
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

                htmlMessage.Bold($"🏗 {commitsStr}")
                    .Bold($" to ").Url(project.WebUrl, $"{project.Name}")
                    .Text(":").Url(treeUrl, $"{branchName}")
                    .Br().Br();

                commits.ForEach(commit =>
                {
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