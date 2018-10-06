using System.Collections.Generic;

namespace RepoStats.Configuration
{
    public class EmailAliases
    {
        public string Primary { get; set; }
        public IEnumerable<string> Aliases { get; set; }
    }
}