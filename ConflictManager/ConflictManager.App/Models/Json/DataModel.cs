using System;

namespace ConflictManager.App.Models.Json
{
    public class DataModel
    {
        public int? Id { get; set; }

        public int Version { get; set; }

        public DateTime Created { get; set; }

        public DateTime Modified { get; set; }

        public string Data { get; set; }
    }
}