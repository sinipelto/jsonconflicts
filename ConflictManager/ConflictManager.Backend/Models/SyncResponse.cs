using ConflictManager.App.Models.Json;

namespace ConflictManager.Backend.Models
{
    public class SyncResponse
    {
        public int StatusCode => (int)Status;

        public Status Status { get; set; }

        public DataModel IncomingModel { get; set; }

        public DataModel CurrentModel { get; set; }

        public DifferenceDetails DifferenceDetails { get; set; }
    }
}