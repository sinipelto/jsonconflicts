using System.Collections.Generic;

namespace ConflictManager.App.Models.Json
{
    public class PassengerShip : Ship
    {
        public int PassengerCountMax { get; set; }

        public List<int> TransmissionCodes { get; set; }
    }
}