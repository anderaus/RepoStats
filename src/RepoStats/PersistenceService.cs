using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.IO;

namespace RepoStats
{
    public class PersistenceService
    {
        public void SaveAsJsonFile(string filename, object content)
        {
            File.WriteAllText(filename,
                JsonConvert.SerializeObject(content, Formatting.Indented, new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                }));
        }
    }
}