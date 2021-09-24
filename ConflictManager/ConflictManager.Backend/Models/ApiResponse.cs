using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ConflictManager.Backend.Models
{
    internal class ApiResponse
    {
        public int StatusCode => (int)Status;

        [JsonConverter(typeof(StringEnumConverter))]
        public Status Status { get; set; }

        public string Response { get; set; }
    }
}