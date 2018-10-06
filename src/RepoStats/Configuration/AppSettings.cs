using System.Collections.Generic;

namespace RepoStats.Configuration
{
    public class AppSettings
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public IEnumerable<Repo> Repos { get; set; }
        public IEnumerable<EmailAliases> AuthorEmailAliases { get; set; }
    }
}