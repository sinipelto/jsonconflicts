using System.Collections.Generic;

namespace ConflictManager.App.Models.Json
{
    internal class Cargo
    {
        public string Type { get; set; }

        public double? TotalWeight { get; set; }

        public string WeightUnit { get; set; }

        public List<Container> Containers { get; set; }
    }

    internal class Container
    {
        public string Contents { get; set; }

        public double? Weight { get; set; }
    }
}