namespace ConflictManager.App.Models.Json
{
    public class CargoShip : Ship
    {
        public Cargo Cargo { get; set; }

        public int ContainersCountLimit { get; set; }
    }
}