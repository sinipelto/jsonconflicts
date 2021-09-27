namespace ConflictManager.Backend.Models
{
    internal class SyncResponse : ApiResponse
    {
        public string IncomingModel { get; set; }

        public string OriginalModel { get; set; }

        public string RawDiff { get; set; }

        //public DifferenceDetails DifferenceDetails { get; set; }
    }
}