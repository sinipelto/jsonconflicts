using ConflictManager.App.Models.Json;
using ConflictManager.App.Services;
using ConflictManager.Backend;
using ConflictManager.Backend.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace ConflictManager.App
{
    internal class Program
    {
        private static void Main()
        {
            // The application starts
            Console.WriteLine("##### START #####");

            //Scenario1();
            Scenario2();
        }

        private static void Scenario2()
        {
            var obj1 = StaticDataService.Ship1;

            var model = new DataModel
            {
                ModuleId = 1,
                Data = JsonConvert.SerializeObject(obj1),
            };

            BackendApi.InsertModule(JsonConvert.SerializeObject(model));

            var obj2 = StaticDataService.Ship1;

            obj2.TransmissionCodes[1] = 991;
            obj2.TransmissionCodes.Add(12938);
            obj2.TransmissionCodes.RemoveAt(0);

            var respStr = BackendApi.EfficientDiff(JsonConvert.SerializeObject(obj2));
            dynamic resp = JsonConvert.DeserializeObject(respStr);

            var inc = resp.IncomingModel;
            var orig = resp.OriginalModel;

            var diffStr = resp.Diff;
            var diff = JsonConvert.DeserializeObject(diffStr);

            Console.WriteLine(diffStr);
            Console.WriteLine(diff[0]);
        }

        private static void Scenario1()
        {
            // Using module ID 1
            const int moduleId = 1;

            // User loads the specific module
            // data of which is passed to the loaded module
            var moduleDataRespStr = BackendApi.GetModuleData(moduleId);
            dynamic moduleDataResp = JsonConvert.DeserializeObject(moduleDataRespStr);

            Console.WriteLine("\r\n");

            // Ensure module data load was ok
            Console.WriteLine(moduleDataResp.Status == "OK"
                ? "FETCH INITIAL MODULE DATA OK."
                : $"FAILED TO FETCH DATA : STATUS: {moduleDataResp.Status}");

            // Parse the actual data string out
            var moduleData = JsonConvert.DeserializeObject(moduleDataResp.Response.ToString());
            var moduleDataStr = moduleData.Data.ToString();

            // Init the module, and pass the data in
            var initModule = new ShipModule(0);
            initModule.SetData(moduleDataStr);

            // Internally alter the data
            initModule.DoSomethingWithTheData(1);

            // Get the altered data
            var initChanged = initModule.GetData();

            // Form an api-model of the data
            var initChangedModel = new DataModel
            {
                ModuleId = moduleData.ModuleId,
                Data = initChanged,
            };

            // Serialized
            var initChangedModelStr = JsonConvert.SerializeObject(initChangedModel);

            // Pass the Data Model in the backend
            var initStoreRespStr = BackendApi.InsertModule(initChangedModelStr);
            dynamic initStoreResp = JsonConvert.DeserializeObject(initStoreRespStr);

            Console.WriteLine("\r\n");

            // Ensure insert result was ok
            Console.WriteLine(initStoreResp.Status != "OK"
                ? $"ERROR: FAILED to store changed model: STATUS: {initStoreResp.Status} RESP: {initStoreResp.Response}"
                : "MODEL STORED TO DB OK.");

            // User sets the app to offline mode
            var syncRespStr = BackendApi.Sync(SyncMode.OnlineToOffline);
            dynamic syncResp = JsonConvert.DeserializeObject(syncRespStr);

            Console.WriteLine("\r\n");

            // Ensure sync was ok (sync to local should never fail except due to errors)
            Console.WriteLine(syncResp.Status == "OK"
                ? "SYNC (ONLINE -> OFFLINE) OK."
                : $"FAILED TO SYNC (ONLINE -> OFFLINE): STATUS: {syncResp.Status}");

            // Fetch the latest data version of the module
            // User loads the specific module
            // data of which is passed to the loaded module
            moduleDataRespStr = BackendApi.GetModuleData(moduleId);
            moduleDataResp = JsonConvert.DeserializeObject(moduleDataRespStr);

            Console.WriteLine("\r\n");

            // Ensure module data load was ok
            Console.WriteLine(moduleDataResp.Status == "OK"
                ? "FETCH SECOND MODULE DATA OK."
                : $"FAILED TO FETCH DATA : STATUS: {moduleDataResp.Status}");

            // Parse the actual data string out
            moduleData = JsonConvert.DeserializeObject(moduleDataResp.Response.ToString());
            moduleDataStr = moduleData.Data.ToString();

            // Instantiate the (offline) module
            var module = new ShipModule(moduleId);
            module.SetData(moduleDataStr);

            // Module internally (User) tampers with its data - makes a change
            module.DoSomethingWithTheData(2);

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

            // Simulate changes on azure db during offline mode
            //BackendApi.SomeoneElseModifyDataDuringOffline(moduleId, "ShipModule");

            // User decides to go back online
            // Sync is called
            var backRespStr = BackendApi.Sync(SyncMode.OfflineToOnline);
            dynamic backResp = JsonConvert.DeserializeObject(backRespStr);

            Console.WriteLine("\r\n");

            Console.WriteLine(backResp.Status == "OK"
                ? "SYNC (OFFLINE -> ONLINE) OK. CHANGES SYNCED."
                : $"FAILED TO SYNC (OFFLINE-> ONLINE): STATUS: {backResp.Status} {backResp.Response}");

            if (backResp.Status == "OK")
            {
                Console.WriteLine($"SYNC OK - Back Online: {backResp.Response}");
                return;
            }

            if (backResp.Status != "CONFLICT")
            {
                Console.WriteLine($"UNKNOWN ERROR DETECTED: {backResp.Status} {backResp.Response}");
            }

            Console.WriteLine("CONFLICT(S) DETECTED.");

            var diffDetails = backResp.DifferenceDetails;
            //var diffDetails = JsonConvert.DeserializeObject(backResp.DifferenceDetails);

            // TODO Resolve conflicts

            Console.WriteLine($"CONFLICTS: {diffDetails.Conflicts}");
            Console.WriteLine($"ONLY LOCAL: {diffDetails.OnlyInLocal}");
            Console.WriteLine($"ONLY AZURE: {diffDetails.OnlyInAzure}");

            //var lmodule = new ShipModule(moduleId);
            //var rmodule = new ShipModule(moduleId);

            //lmodule.SetData(backRespResp.OnlyInLocal);
            //lmodule.SetConflictData(conflicts, "Local");

            //rmodule.SetData(backRespResp.OnlyInAzure);
            //rmodule.SetConflictData(conflicts, "Azure");


            // Key -> conflicted property name
            // Value -> from which to take
            var userDecided = new Dictionary<string, string>
            {
                {"Engines", "Local"},
                {"TransmissionCodes", "Azure"},
                //{"", ""},
                //{"", ""},
            };

            dynamic mergedData = new JObject();

            var confs = diffDetails.Conflicts;
            var az = diffDetails.OnlyInAzure;
            var loc = diffDetails.OnlyInLocal;

            //foreach (var confProp in diffDetails.ConflictingProperties)
            //{
            //    if (userDecided.ContainsKey(confProp))
            //    {
            //        mergedData[confProp] = confs[confProp][userDecided[confProp]];
            //    }
            //}

            // Go through the user chosen properties
            foreach (var (key, value) in userDecided)
            {
                // If the property is conflicted
                // Pick it from the requested side
                if (confs[key] != null)
                {
                    mergedData[key] = confs[key][value];
                }
                // If the property is only avail in Azure, and is requested (picked) from Azure side, pick it
                else if (az[key] != null && value == "Azure")
                {
                    mergedData[key] = az[key];
                }
                // If the property is only avail in Local, and is requested (picked) from Local side, pick it
                else if (loc[key] != null && value == "Local")
                {
                    mergedData[key] = loc[key];
                }
                // If the requested property is not available at all -> Impossible situation
                else
                {
                    throw new InvalidOperationException("User requested property not available.");
                }
            }

            Console.WriteLine($"RESOLVED DATA: {mergedData}");

            var mergeModel = new DataModel
            {
                ModuleId = moduleId,
                Data = JsonConvert.SerializeObject(mergedData),
            };

            // Let the api know the conflict is resolved, and to use the resolved data as new, final version
            var resolveRespStr = BackendApi.ResolveConflict(JsonConvert.SerializeObject(mergeModel));
            dynamic resolveResp = JsonConvert.DeserializeObject(resolveRespStr);

            Console.WriteLine(resolveResp.Status == "OK" ? $"CONFLICT RESOLVED OK: {resolveResp.Response}" : $"ERROR WHILE RESOLVING CONFLICT: {resolveResp.Status} {resolveResp.Response}");
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
