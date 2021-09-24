using ConflictManager.App.Models.Json;
using ConflictManager.App.Services;
using ConflictManager.Backend;
using ConflictManager.Backend.Models;
using Newtonsoft.Json;
using System;

namespace ConflictManager.App
{
    internal class Program
    {
        private static void Main()
        {
            // The application starts
            Console.WriteLine("##### START #####");

            Scenario1();
        }

        private static void Scenario1()
        {
            // User sets the app to offline mode
            var syncRespStr = BackendApi.Sync(SyncMode.OnlineToOffline);
            dynamic syncResp = JsonConvert.DeserializeObject(syncRespStr);

            Console.WriteLine("\r\n");

            // Ensure sync was ok (sync to local should never fail except due to errors)
            Console.WriteLine(syncResp.Status == "OK"
                ? "SYNC (ONLINE -> OFFLINE) OK."
                : $"FAILED TO SYNC (ONLINE -> OFFLINE): STATUS: {syncResp.Status}");

            // Using module ID 1
            const int moduleId = 1;

            // User loads the specific module
            // Which data is passed to the loaded module
            var moduleDataRespStr = BackendApi.GetModuleData(moduleId);
            dynamic moduleDataResp = JsonConvert.DeserializeObject(moduleDataRespStr);
            var moduleData = JsonConvert.DeserializeObject(moduleDataResp.Response.ToString());

            // Instantiate the module
            var module = new ShipModule(moduleId);
            module.SetData(moduleData.Data.ToString());

            // Module internally (User) tampers with its data - makes a change
            module.DoSomethingWithTheData();

            // User saves the changes to db:
            var changedData = module.GetData();

            // Data model formed from the received data from the module
            var changedModel = new DataModel
            {
                ModuleId = moduleData.ModuleId,
                Data = changedData,
            };

            // Serialized
            var changedModelStr = JsonConvert.SerializeObject(changedModel);

            // Data Stored in the backend
            var storeRespStr = BackendApi.InsertModule(changedModelStr);
            dynamic storeResp = JsonConvert.DeserializeObject(storeRespStr);

            Console.WriteLine("\r\n");

            Console.WriteLine(storeResp.Status != "OK"
                ? $"ERROR: FAILED to store changed model: STATUS: {storeResp.Status} RESP: {storeResp.Response}"
                : "MODEL STORED TO DB OK.");

            Console.WriteLine("\r\n");

            // User decides to go back online
            // Sync is called
            var backRespStr = BackendApi.Sync(SyncMode.OfflineToOnline);
            dynamic backResp = JsonConvert.DeserializeObject(backRespStr);

            Console.WriteLine("\r\n");

            Console.WriteLine(backResp.Status == "OK"
                ? "SYNC (OFFLINE -> ONLINE) OK. CHANGES SYNCED."
                : $"FAILED TO SYNC (OFFLINE-> ONLINE): STATUS: {backResp.Status} {backResp.Response}");

            if (backResp.Status != "CONFLICT")
            {
                Console.WriteLine($"UNKNOWN ERROR DETECTED: {backResp.Status} {backResp.Response}");
            }

            Console.WriteLine("CONFLICT DETECTED.");

            // TODO Resolve conflicts

            var resolvedModel = new DataModel
            {

            };

            // Let the api know the conflict is resolved, and to use the resolved data as final
        }

        private static void Test()
        {
            var str1 = JsonConvert.SerializeObject(StaticDataService.Eng1);
            var str2 = JsonConvert.SerializeObject(StaticDataService.Eng2);

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
    }
}
