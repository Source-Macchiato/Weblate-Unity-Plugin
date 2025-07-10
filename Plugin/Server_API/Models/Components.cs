using Newtonsoft.Json;

namespace Weblate.Plugin.Server_API.Models
{
    // /api/projects/(string:project)/components/
    public class Components
    {
        [JsonProperty("results")]
        public ComponentsResults[] Results { get; set; }
    }
    
    public class ComponentsResults
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("slug")]
        public string Slug { get; set; }
    }
}