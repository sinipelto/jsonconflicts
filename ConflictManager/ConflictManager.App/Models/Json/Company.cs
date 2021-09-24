namespace ConflictManager.App.Models.Json
{
    public class Company
    {
        public string Name { get; set; }

        public Owner Owner { get; set; }

        public double? Value { get; set; }

        public string Location { get; set; }
    }
}