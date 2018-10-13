using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RepoStats.Configuration;
using RepoStats.Models;
using System;
using System.Linq;

namespace RepoStats
{
    public class App
    {
        private readonly ILogger<App> _logger;
        private readonly AppSettings _appSettings;
        private readonly RepoParser _repoParser;
        private readonly PersistenceService _persistenceService;

        public App(
            ILogger<App> logger,
            IOptions<AppSettings> appSettings,
            RepoParser repoParser,
            PersistenceService persistenceService)
        {
            _logger = logger;
            _appSettings = appSettings.Value;
            _repoParser = repoParser;
            _persistenceService = persistenceService;
        }

        public void Run()
        {
            var result = new ResultModel();

            foreach (var repo in _appSettings.Repos.OrderBy(r => r.FriendlyName))
            {
                _repoParser.LoadRepo(repo);

                var repoResult = _repoParser.CollectCommitStatistics();
                repoResult.Name = repo.FriendlyName;
                result.Repositories.Add(repoResult);

                _logger.LogDebug($"Most recent commit at {repoResult.LatestCommit}");

                foreach (var contributor in repoResult.ContributorCommitCounts)
                {
                    _logger.LogInformation($"\t{contributor.Email} has made {contributor.CommitCount} commits");
                }
            }

            result.Created = DateTime.UtcNow;

            _persistenceService.SaveAsJsonFile("output.json", result);
        }
    }
}