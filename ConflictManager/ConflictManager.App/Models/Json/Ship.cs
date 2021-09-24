using System.Collections.Generic;

namespace ConflictManager.App.Models.Json
{
    public class Ship
    {
        public string Name { get; set; }

        public double? Length { get; set; }

        public Owner Owner { get; set; }

        public List<Engine> Engines { get; set; }

        public List<Ship> Companions { get; set; }
    }
}