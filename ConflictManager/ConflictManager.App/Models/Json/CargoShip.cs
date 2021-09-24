namespace ConflictManager.App.Models.Json
{
    internal class CargoShip : Ship
    {
        public Cargo Cargo { get; set; }

        public int ContainersCountLimit { get; set; }
    }
}