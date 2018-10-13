using System;
using System.Collections.Generic;

namespace RepoStats.Models
{
    public class RepositoryModel
    {
        public string Name { get; set; }
        public DateTimeOffset LatestCommit { get; set; }
        public List<Contributor> ContributorCommitCounts { get; set; } = new List<Contributor>();
    }
}