using Newtonsoft.Json.Linq;

namespace ConflictManager.Backend.Models
{
    internal class Conflict
    {
        public JToken Azure { get; set; }

        public JToken Local { get; set; }
    }
}