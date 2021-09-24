using System;

namespace ConflictManager.Backend.Models
{
    public class DataModel
    {
        public int? ModuleId { get; set; }

        public int Hash { get; set; }

        public int Version { get; set; }

        public DateTime Created { get; set; }

        public DateTime Modified { get; set; }

        public string Data { get; set; }
    }
}