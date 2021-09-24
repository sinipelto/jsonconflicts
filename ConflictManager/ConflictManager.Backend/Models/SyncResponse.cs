namespace ConflictManager.Backend.Models
{
    internal class SyncResponse : ApiResponse
    {
        public DataModel IncomingModel { get; set; }

        public DataModel CurrentModel { get; set; }

        public DifferenceDetails DifferenceDetails { get; set; }
    }
}