using ConflictManager.App.Models.Json;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ConflictManager.App
{
    internal class Program
    {
        private static void Main()
        {
            Console.WriteLine("##### START #####");

            var module = BackendApi.GetModule(1);

            var modelStr = JsonConvert.SerializeObject(model);

            var respStr = BackendApi.Sync(modelStr);

            Console.WriteLine("\r\n");

            Console.WriteLine($"RESP: {respStr}");

            dynamic resp = JsonConvert.DeserializeObject(respStr);

            if (resp["Status"] == "OK")
            {
                Console.WriteLine("STATUS OK - NO CONFLICTS");
                return;
            }

            Console.WriteLine("CONFLICTS FOUND");

            dynamic conflictObj;

            conflictObj[""]

            // TODO Resolve conflicts

            var resolved = new DataModel
            {

            }

            return;

            var str1 = JsonConvert.SerializeObject(StaticDataService.eng1);
            var str2 = JsonConvert.SerializeObject(StaticDataService.eng2);

            Console.WriteLine("LEFT:\r\n");
            Console.WriteLine(str1);

            Console.WriteLine("\r\n");

            Console.WriteLine("RIGHT:\r\n");
            Console.WriteLine(str2);

            Console.WriteLine("\r\n");

            //var diffs = SyncService.CompareData(str1, str2);

            //Console.WriteLine($"\r\nDIFF DETAILS:\r\n{JsonConvert.SerializeObject(diffs)}");
            //Console.WriteLine($"DIFF:\r\n{(diffs == null ? "NULL" : string.Join('\n', diffs))}");

            Console.WriteLine("\r\n");
        }

        private static class StaticDataService
        {
            public static Engine eng1 => new()
            {
                Name = "Generic Engine 231",
                MaxTemp = 99.85,
                MinTemp = 3.22,
                States = new List<string> { "Offline", "Online", "Restarting", "Charging" },
            };

            public static Engine eng2 => new()
            {
                Name = "Generic Engine 99",
                MaxTemp = 99.85,
                MinTemp = 5.38,
                States = new List<string> { "Online", "Offline", "Restarting", "Charging" },
            };

            public static TurboEngine te => new()
            {
                Name = "Turbo Engine",
                MaxTemp = 995.1,
                MinTemp = 30.9,
                AirFlow = 882,
                States = new List<string> { "Offline", "Online", "Restarting", "Malfunction" },
            };

            public static DieselEngine de => new()
            {
                Name = "Diesel Engine",
                MaxTemp = 95.19,
                MinTemp = 10.91,
                FuelLevel = 88.90,
                FuelLevelMax = 100.00,
                States = new List<string> { "Offline", "Online", "Refueling", "OutOfFuel" },
            };

            public static Company comp1 => new()
            {
                Name = "No Company",
                Value = 0,
                Location = null,
                Owner = null,
            };

            public static Owner ow1 => new()
            {
                Name = "Firstname Lname",
                Type = "Individual",
                CompanyDetails = comp1,
            };

            public static Company cdet => new()
            {
                Name = "Some Company Co",
                Value = 21398129,
                Location = "Somewhere",
                Owner = ow1,
            };

            public static Company cdet2 => new()
            {
                Name = "Some Another Company Co",
                Value = 21398129,
                Location = "Somewhere Else Street 1, Finland",
                Owner = ow1,
            };

            public static Owner ow2 => new()
            {
                Name = "Some Owner Company",
                Type = "Company",
                CompanyDetails = cdet,
            };

            public static PassengerShip ship1 => new()
            {
                Name = "Test Ship 128",
                Length = 60.15,
                Engines = new List<Engine> { eng1, eng2, de },
                Companions = null,
                Owner = ow1,
                PassengerCountMax = 572,
                TransmissionCodes = new List<int> { 282, 123897, 21487, 161 }
            };

            public static Container cont => new()
            {
                Contents = "Laptops",
                Weight = 5298.22,
            };

            public static Cargo carg => new()
            {
                TotalWeight = 99427.12,
                WeightUnit = "Kilograms",
                Type = "Consumer Electronics",
                Containers = new List<Container> { cont }
            };

            public static CargoShip ship2 => new()
            {
                Name = "Test Ship 14",
                Length = 53.3,
                Engines = new List<Engine> { eng2, eng1, de },
                Companions = new List<Ship> { ship1 },
                Owner = ow2,
                Cargo = carg,
                ContainersCountLimit = 1339,
            };
        }

    }
}
