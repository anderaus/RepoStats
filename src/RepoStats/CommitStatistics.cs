using LibGit2Sharp;
using System.Collections.Generic;

namespace RepoStats
{
    public class CommitStatistics
    {
        public List<string> UniqueAuthorEmails { get; set; } = new List<string>();
        public Commit MostRecentCommit { get; set; }
    }
}