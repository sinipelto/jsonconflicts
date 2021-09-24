using ConflictManager.App.Models.Json;
using ConflictManager.Backend.Models;
using JsonDiffPatchDotNet;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace ConflictManager.Backend
{
    /// <summary>
    /// Simulates the connection to the backend.
    /// Handles only JSON strings as inputs and outputs.
    /// Static methods represent HTTP API methods.
    /// </summary>
    public static class BackendApi
    {
        private static bool _inCloud = true;

        /// <summary>
        /// Syncs the remote (local) model with azure counterpart.
        /// Detects any conflicts bewtween the data models, and returns response according to the results.
        /// </summary>
        /// <param name="incomingModel">The model from local DB to be compared against Azure DB.</param>
        /// <param name="mode">Syncing from online to offline or from offline to online -> affects the syncing behaviour.</param>
        /// <returns>Response on any conflicts or OK response if no conflicts detected during sync.</returns>
        public static string Sync(SyncMode mode)
        {
            if (_inCloud && mode == SyncMode.OfflineToOnline)
            {
                return "ERROR: Cannot move Offline -> Online: Already online.";
            }

            if (!_inCloud && mode == SyncMode.OnlineToOffline)
            {
                return "ERROR: Cannot move Online -> Offline: Already offline.";
            }

            if (mode == SyncMode.OnlineToOffline)
            {
                LocalDb.Truncate(); // Truncate Local DB
                LocalDb.InsertMany(RemoteDb.GetAll()); // Copy Remote DB to Local DB
                _inCloud = false; // Go To Offline
            }

            if (mode == SyncMode.OfflineToOnline)
            {
                // Ensure we have a valid connection to remote DB
                if (!EnsureConnected())
                    throw new ApplicationException("ERROR: Connection failure. No access to remote DB.");

                // In real scenario, for each table, collect any changed ones

                const int id = 1; // The ID of the model modified in both dbs

                // Fetch the latest versions and check for conflicts
                var localLatest = LocalDb.Get(id);

                var resp = SyncService.Sync(localLatest);

                return JsonConvert.SerializeObject(resp);
            }

            throw new InvalidOperationException("Unhandled situation.");
        }

        /// <summary>
        /// A special method that should be called after the conflicts are resolved between the conflicting models.
        /// Forcibly creates a new version on the cloud DB with the contents of the constructed resolution model data.
        /// </summary>
        /// <param name="id">ID of the module to be altered.</param>
        /// <param name="data">The JSON data of the model that has its conflicts resolved.</param>
        /// <param name="model">TODO xyz/param>
        /// <returns>The model if the insertion succeeds, otherwise the details of the error occurred.</returns>
        public static string InsertModule(string model)
        {
            var resp = InsertModule(JsonConvert.DeserializeObject<DataModel>(model));
            return JsonConvert.SerializeObject(resp);
        }

        /// <summary>
        /// Returns the module that corresponds with the given ID.
        /// </summary>
        /// <returns></returns>
        public static string GetModule(int id)
        {
            var model = _inCloud ? RemoteDb.Get(id) : LocalDb.Get(id);
            return JsonConvert.SerializeObject(model);
        }

        private static DataModel InsertModule(DataModel model)
        {
            var ins = _inCloud ? RemoteDb.Upsert(model) : LocalDb.Upsert(model);
            return ins;
        }

        public static readonly Database LocalDb = new();

        public static readonly Database RemoteDb = new();

        private static bool EnsureConnected() => true;

        private static class SyncService
        {
            public static SyncResponse Sync(DataModel incomingModel)
            {
                Debug.Assert(incomingModel.Id != null, "incomingModel.Id != null");

                // Fetch local model from own DB (Cloud)
                var remoteModel = RemoteDb.Get((int)incomingModel.Id);

                var resp = new SyncResponse
                {
                    Status = Status.OK,
                    IncomingModel = incomingModel,
                };

                if (remoteModel == null)
                {
                    resp.Status = Status.NOTFOUND;
                    return resp;
                }

                resp.CurrentModel = remoteModel;

                // Check if the cloud version is ahead the incoming version (always treated as no incoming changes)
                if (remoteModel.Version >= incomingModel.Version)
                {
                    // Nothing changed in local, do nothing and return sync ok
                    return resp;
                }

                // Check the difference between the azure and incoming model datas
                var diff = CompareData(remoteModel.Data, incomingModel.Data);

                // If no conflicts, insert the incoming model into db
                if (diff == null)
                {
                    RemoteDb.Upsert(incomingModel);
                    return resp;
                }

                resp.Status = Status.CONFLICT;
                resp.DifferenceDetails = diff;

                return resp;
            }

            /// <summary>
            /// String overload for the parser.
            /// </summary>
            /// <param name="localData">First JSON object as string.</param>
            /// <param name="remoteData">Second JSON object as string.</param>
            /// <returns>NULL if there aren't any differences. Otherwise returns the differencing parts between the two JSON objects.</returns>
            private static DifferenceDetails CompareData(string localData, string remoteData)
            {
                return CompareData(JToken.Parse(localData), JToken.Parse(remoteData));
            }

            /// <summary>
            /// Checks if the compared JSON values have any differences
            /// Returns the differencing properties with the differing parts.
            /// </summary>
            /// <param name="localDbData">First JSON object</param>
            /// <param name="azureDbData">Second JSON object</param>
            /// <returns>NULL if there aren't any differences. Otherwise returns the differencing parts between the two JSON objects.</returns>
            private static DifferenceDetails CompareData(JToken localDbData, JToken azureDbData)
            {
                var tool = new JsonDiffPatch(new Options
                {
                    ArrayDiff = ArrayDiffMode.Simple,
                    TextDiff = TextDiffMode.Simple,
                    MinEfficientTextDiffLength = 0,
                });

                var diff = tool.Diff(localDbData, azureDbData);

                if (diff == null) return null;

                // diff != null -> diff(s) exist

                var details = new DifferenceDetails
                {
                    AzureOnlyProperties = new List<string>(),
                    OnlyInAzure = new Dictionary<string, JToken>(),
                    LocalOnlyProperties = new List<string>(),
                    OnlyInLocal = new Dictionary<string, JToken>(),
                    ConflictingProperties = new List<string>(),
                    Conflicts = new Dictionary<string, Conflict>()
                };

                var diffs = ((JObject)diff).Properties().ToList();
                Console.WriteLine($"\r\nDIFF PROPS:\r\n{(string.Join("\r\n", diffs))}");

                // Diff Root = always object
                CompareObject(details, (JObject)diff);

                //return diffProperties;

                return details;
            }

            private static void CompareObject(DifferenceDetails details, JObject jsonObject)
            {
                foreach (var prop in jsonObject.Properties())
                {
                    // Process json objects in deeper level
                    // direct objects, objects inside lists

                    var y1 = prop.Path;
                    var y2 = prop.Values().FirstOrDefault()?.Path;
                    var x1 = prop.Name;
                    var x2 = prop.Value;
                    var x3 = prop.Type;
                    var x4 = prop.HasValues;
                    var x5 = prop.Value.First;
                    var x6 = prop.Value.Type;
                    var x7 = prop.Value.Values();
                    var x8 = prop.Count;
                    var x9 = prop.HasValues;
                    var x10 = prop.Children().ToList();

                    // Collect basic info from property
                    var path = prop.Path;
                    var type = prop.Value.Type;
                    var count = prop.Value.Count();
                    var values = prop.Values().ToList();

                    // values = all properties (in the current property object) that have different value

                    // Diff (remoteData, localData)

                    // Size <= 0 => not possible
                    if (!prop.HasValues) throw new InvalidDataException("Count cannot be less than 1!");

                    if (type == JTokenType.Object)
                    {
                        CompareObject(details, (JObject)prop.Value);
                        continue;
                    }

                    var leftVal = values[0]; // 1st

                    // If type == Array && size == 1 -> value only in localData (left) but not in remoteData (right)
                    // contains the value in right only
                    if (type == JTokenType.Array && count == 1)
                    {
                        details.LocalOnlyProperties.Add(path);
                        details.OnlyInLocal.Add(path,
                            leftVal.Type == JTokenType.Null ? null : leftVal);

                        continue;
                    }

                    var rightVal = values[1]; // 2nd

                    // If type == array && size == 2 -> property exists in both, but null/different on other -> Conflict on simple property
                    if (type == JTokenType.Array && count == 2)
                    {
                        details.ConflictingProperties.Add(path);
                        details.Conflicts.Add(path, new Conflict { Local = leftVal.Type == JTokenType.Null ? null : leftVal, Azure = rightVal.Type == JTokenType.Null ? null : rightVal });

                        continue;
                    }

                    // If type == object && Size == 3 -> Property is object with different sub-properties or property values

                    // If size >= 3 && 2nd, 3rd, 4th... == missing properties -> new properties in left, missing in right, listed

                    var diffPart = values[2]; // 3rd

                    // If type == array && size == 3 && 2nd == 0, 3rd == 0 -> simple property in remote (left), not in local (right)
                    // Values exists in remote, doesnt exist in local
                    if (
                        type == JTokenType.Array
                        &&
                        leftVal != rightVal
                        &&
                        rightVal.Type == JTokenType.Integer && rightVal.Value<int>() == 0
                        &&
                        diffPart.Type == JTokenType.Integer && diffPart.Value<int>() == 0
                        )
                    {
                        details.AzureOnlyProperties.Add(path);
                        details.OnlyInAzure.Add(path, leftVal.Type == JTokenType.Null ? null : leftVal);
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Error.WriteLine("WENT ELSE");
                    }
                }
            }
        }

        public class Database
        {
            public IEnumerable<DataModel> GetAll() => _data.AsEnumerable();

            public DataModel Get(int id, int? version = null)
            {
                return
                    version == null
                    ? _data.Where(i => i.Id == id).OrderByDescending(i => i.Version).FirstOrDefault()
                    : _data.Where(i => i.Id == id).FirstOrDefault(i => i.Version == version);
            }

            public void InsertMany(IEnumerable<DataModel> models)
            {
                foreach (var dataModel in models)
                {
                    Upsert(dataModel);
                }
            }

            public DataModel Upsert(DataModel model)
            {
                if (model.Id != null) throw new ArgumentNullException();

                var val = _data.Where(i => i.Id == model.Id).OrderByDescending(i => i.Version).FirstOrDefault();

                // Greatest ID or 0
                var idMax = _data.Max(i => i.Id) ?? 0;

                // ID does not exist
                if (val == null)
                {
                    val = new DataModel
                    {
                        Id = idMax + 1,
                        Created = DateTime.UtcNow,
                        Modified = DateTime.UtcNow,
                        Version = 0,
                        Data = model.Data,
                    };

                    _data.Add(val);
                }
                else
                {
                    val.Id = model.Id;
                    val.Modified = DateTime.UtcNow;
                    val.Version = ++val.Version;
                    val.Data = model.Data;
                }

                return val;
            }

            public void Truncate() => _data.Clear();

            private readonly Collection<DataModel> _data = new()
            {
                new DataModel
                {
                    Id = 2,
                    Created = DateTime.UtcNow,
                    Modified = DateTime.UtcNow,
                    Version = 0,
                    Data = null,
                },
                new DataModel
                {
                    Id = 1,
                    Created = DateTime.UtcNow,
                    Modified = DateTime.UtcNow,
                    Version = 0,
                    Data = null,
                },
                new DataModel
                {
                    Id = 1,
                    Created = DateTime.UtcNow,
                    Modified = DateTime.UtcNow,
                    Version = 1,
                    Data = null,
                },
            };
        }
    }
}