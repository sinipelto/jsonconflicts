using Newtonsoft.Json.Linq;

namespace ConflictManager.Backend.Models
{
    public class Conflict
    {
        public JToken Azure { get; set; }

        public JToken Local { get; set; }
    }
}