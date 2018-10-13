using LibGit2Sharp;
using LibGit2Sharp.Handlers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RepoStats.Configuration;
using RepoStats.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RepoStats
{
    public class RepoParser : IDisposable
    {
        private readonly ILogger<RepoParser> _logger;
        private readonly AppSettings _settings;
        private Repository _repo;
        private bool _disposed;

        public RepoParser(ILogger<RepoParser> logger, IOptions<AppSettings> options)
        {
            _logger = logger;
            _settings = options.Value;
        }

        public void LoadRepo(Repo repoToLoad)
        {
            _repo?.Dispose();

            var localRepoRelativePath = repoToLoad.FriendlyName.ToLower().Replace(" ", "_");
            var localRepoAbsolutePath = Path.Combine(AppContext.BaseDirectory, "reposcache", localRepoRelativePath);

            CredentialsHandler credentialsHandler = null;
            if (!string.IsNullOrEmpty(_settings.Username))
            {
                credentialsHandler = (url, usernameFromUrl, types) =>
                    new UsernamePasswordCredentials
                    {
                        Username = _settings.Username,
                        Password = _settings.Password
                    };
            }

            bool recentlyCloned = false;
            if (!Directory.Exists(localRepoAbsolutePath))
            {
                var cloneOptions = new CloneOptions();
                if (credentialsHandler != null)
                {
                    cloneOptions.CredentialsProvider = credentialsHandler;
                }

                Repository.Clone(repoToLoad.Url, localRepoAbsolutePath, cloneOptions);
                _logger.LogInformation($"Cloned {repoToLoad.Url} to folder {localRepoAbsolutePath}");
                recentlyCloned = true;
            }

            _repo = new Repository(localRepoAbsolutePath);

            if (!recentlyCloned)
            {
                var fetchOptions = new FetchOptions();
                if (credentialsHandler != null)
                {
                    fetchOptions.CredentialsProvider = credentialsHandler;
                }
                var remote = _repo.Network.Remotes.First(r => r.Name == "origin");
                var refSpecs = remote.FetchRefSpecs.Select(x => x.Specification);
                Commands.Fetch(_repo, remote.Name, refSpecs, fetchOptions, "");
                _logger.LogInformation($"Fetched origin for {repoToLoad.FriendlyName} in folder {localRepoAbsolutePath}");
            }
        }

        public RepositoryModel CollectCommitStatistics()
        {
            var repoResult = new RepositoryModel();
            var commitsPerAuthor = new Dictionary<string, List<Commit>>();

            foreach (var branch in _repo.Branches)
            {
                _logger.LogDebug($"Working on branch {branch.FriendlyName}");

                var mostRecentCommitOnBranch = branch.Commits.FirstOrDefault();
                if (mostRecentCommitOnBranch != null
                    && (repoResult.LatestCommit == default ||
                        mostRecentCommitOnBranch.Author.When > repoResult.LatestCommit))
                {
                    repoResult.LatestCommit = mostRecentCommitOnBranch.Author.When;
                }

                foreach (var commit in branch.Commits.Where(IsNotAMergeCommit))
                {
                    var authorEmail = MapToPrimaryEmail(commit.Author.Email);
                    if (commitsPerAuthor.ContainsKey(authorEmail))
                    {
                        if (!commitsPerAuthor[authorEmail].Contains(commit))
                        {
                            commitsPerAuthor[authorEmail].Add(commit);
                        }
                    }
                    else
                    {
                        commitsPerAuthor.Add(authorEmail, new List<Commit> { commit });
                    }
                }
            }

            repoResult.ContributorCommitCounts.AddRange(
                commitsPerAuthor
                    .OrderByDescending(pair => pair.Value.Count)
                    .Select(pair =>
                        new KeyValuePair<string, int>(pair.Key, pair.Value.Count)));

            return repoResult;
        }

        private string MapToPrimaryEmail(string email)
        {
            var matchingPrimary = _settings.AuthorEmailAliases
                .FirstOrDefault(e =>
                    e.Aliases.Any(a => a.Equals(email, StringComparison.InvariantCultureIgnoreCase)));

            return matchingPrimary?.Primary.ToLower() ?? email.ToLower();
        }

        private static bool IsNotAMergeCommit(Commit commit) => commit.Parents.Count() <= 1;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _repo.Dispose();
                }
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
    }
}