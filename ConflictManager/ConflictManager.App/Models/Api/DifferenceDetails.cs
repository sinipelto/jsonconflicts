using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace ConflictManager.App.Models.Api
{
    public class DifferenceDetails
    {
        public List<string> ConflictingProperties { get; set; }

        public Dictionary<string, Conflict> Conflicts { get; set; }

        public List<string> LocalOnlyProperties { get; set; }

        public Dictionary<string, JToken> OnlyInLocal { get; set; }

        public List<string> AzureOnlyProperties { get; set; }

        public Dictionary<string, JToken> OnlyInAzure { get; set; }
    }
}