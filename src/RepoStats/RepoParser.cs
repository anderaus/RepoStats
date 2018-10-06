using LibGit2Sharp;
using LibGit2Sharp.Handlers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RepoStats.Configuration;
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
                    new UsernamePasswordCredentials()
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

        public CommitStatistics CollectCommitStatistics()
        {
            var stats = new CommitStatistics();

            foreach (var branch in _repo.Branches)
            {
                _logger.LogDebug($"Branch {branch.FriendlyName}");

                var mostRecentCommit = branch.Commits.FirstOrDefault();
                if (mostRecentCommit != null
                    && (stats.MostRecentCommit == null || mostRecentCommit.Author.When > stats.MostRecentCommit.Author.When))
                {
                    stats.MostRecentCommit = mostRecentCommit;
                }

                var uniqueAuthors = GetUniqueAuthorEmails(branch.Commits);
                foreach (var email in uniqueAuthors.OrderBy(a => a))
                {
                    if (!stats.UniqueAuthorEmails.Contains(email, StringComparer.InvariantCultureIgnoreCase))
                    {
                        stats.UniqueAuthorEmails.Add(email.ToLower());
                    }
                }
            }

            return stats;
        }

        private static IEnumerable<string> GetUniqueAuthorEmails(ICommitLog commitLog)
        {
            return commitLog
                .DistinctBy(c => c.Author.Email)
                .Select(c => c.Author.Email).ToList();
        }

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