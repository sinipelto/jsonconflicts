using System.Collections.Generic;

namespace ConflictManager.App.Models.Json
{
    public class Engine
    {
        public string Name { get; set; }

        public double? MaxTemp { get; set; }

        public double? MinTemp { get; set; }
        
        public List<string> States { get; set; }
    }
}