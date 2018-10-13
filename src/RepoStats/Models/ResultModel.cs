using System;
using System.Collections.Generic;

namespace RepoStats.Models
{
    public class ResultModel
    {
        public DateTime Created { get; set; }
        public List<RepositoryModel> Repositories { get; set; } = new List<RepositoryModel>();
    }
}