using ConflictManager.App.Models.Json;
using System.Collections.Generic;

namespace ConflictManager.App.Services
{
    internal static class StaticDataService
    {
        public static Engine Eng1 => new()
        {
            Name = "Generic Engine 231",
            MaxTemp = 99.85,
            MinTemp = 3.22,
            States = new List<string> { "Offline", "Online", "Restarting", "Charging" },
        };

        public static Engine Eng2 => new()
        {
            Name = "Generic Engine 99",
            MaxTemp = 99.85,
            MinTemp = 5.38,
            States = new List<string> { "Online", "Offline", "Restarting", "Charging" },
        };

        public static TurboEngine Te => new()
        {
            Name = "Turbo Engine",
            MaxTemp = 995.1,
            MinTemp = 30.9,
            AirFlow = 882,
            States = new List<string> { "Offline", "Online", "Restarting", "Malfunction" },
        };

        public static DieselEngine De => new()
        {
            Name = "Diesel Engine",
            MaxTemp = 95.19,
            MinTemp = 10.91,
            FuelLevel = 88.90,
            FuelLevelMax = 100.00,
            States = new List<string> { "Offline", "Online", "Refueling", "OutOfFuel" },
        };

        public static Company Comp1 => new()
        {
            Name = "No Company",
            Value = 0,
            Location = null,
            Owner = null,
        };

        public static Owner Ow1 => new()
        {
            Name = "Firstname Lname",
            Type = "Individual",
            CompanyDetails = Comp1,
        };

        public static Company Cdet => new()
        {
            Name = "Some Company Co",
            Value = 21398129,
            Location = "Somewhere",
            Owner = Ow1,
        };

        public static Company Cdet2 => new()
        {
            Name = "Some Another Company Co",
            Value = 21398129,
            Location = "Somewhere Else Street 1, Finland",
            Owner = Ow1,
        };

        public static Owner Ow2 => new()
        {
            Name = "Some Owner Company",
            Type = "Company",
            CompanyDetails = Cdet,
        };

        public static PassengerShip Ship1 => new()
        {
            Name = "Test Ship 128",
            Length = 60.15,
            Engines = new List<Engine> { Eng1, Eng2, De },
            Companions = null,
            Owner = Ow1,
            PassengerCountMax = 572,
            TransmissionCodes = new List<int> { 282, 123897, 21487, 161 }
        };

        public static Container Cont => new()
        {
            Contents = "Laptops",
            Weight = 5298.22,
        };

        public static Cargo Cargo => new()
        {
            TotalWeight = 99427.12,
            WeightUnit = "Kilograms",
            Type = "Consumer Electronics",
            Containers = new List<Container> { Cont }
        };

        public static CargoShip Ship2 => new()
        {
            Name = "Test Ship 14",
            Length = 53.3,
            Engines = new List<Engine> { Eng2, Eng1, De },
            Companions = new List<Ship> { Ship1 },
            Owner = Ow2,
            Cargo = Cargo,
            ContainersCountLimit = 1339,
        };
    }
}