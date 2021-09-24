using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace ConflictManager.Backend.Models
{
    internal class DifferenceDetails
    {
        public List<string> ConflictingProperties { get; set; }

        public Dictionary<string, Conflict> Conflicts { get; set; }

        public List<string> LocalOnlyProperties { get; set; }

        public Dictionary<string, JToken> OnlyInLocal { get; set; }

        public List<string> AzureOnlyProperties { get; set; }

        public Dictionary<string, JToken> OnlyInAzure { get; set; }
    }
}