using Newtonsoft.Json.Linq;

namespace ConflictManager.App.Models.Api
{
    public class Conflict
    {
        public JToken Azure { get; set; }

        public JToken Local { get; set; }
    }
}