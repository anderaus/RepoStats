using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RepoStats.Configuration;

namespace RepoStats
{
    public class App
    {
        private readonly ILogger<App> _logger;
        private readonly AppSettings _appSettings;
        private readonly RepoParser _repoParser;

        public App(ILogger<App> logger, IOptions<AppSettings> appSettings, RepoParser repoParser)
        {
            _logger = logger;
            _appSettings = appSettings.Value;
            _repoParser = repoParser;
        }

        public void Run()
        {
            foreach (var repo in _appSettings.Repos)
            {
                _repoParser.LoadRepo(repo);

                var stats = _repoParser.CollectCommitStatistics();

                _logger.LogDebug($"Most recent commit at {stats.MostRecentCommit.Author.When} by {stats.MostRecentCommit.Author.Email}");

                foreach (var email in stats.UniqueAuthorEmails)
                {
                    _logger.LogInformation(email);
                }
            }
        }
    }
}