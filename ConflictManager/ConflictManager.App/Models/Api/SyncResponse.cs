using ConflictManager.App.Models.Json;

namespace ConflictManager.App.Models.Api
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