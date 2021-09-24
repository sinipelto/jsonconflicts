using ConflictManager.Backend.Data;
using ConflictManager.Backend.Models;
using ConflictManager.Backend.Services;
using Newtonsoft.Json;
using System;

namespace ConflictManager.Backend
{
    /// <summary>
    /// Simulates the connection to the backend.
    /// Handles only JSON strings as inputs and outputs.
    /// Static methods represent HTTP API methods.
    /// </summary>
    public static class BackendApi
    {
        // Mocking the connection status to always succeed
        private static bool EnsureConnected() => true;

        // Initially, the backend is running in the cloud instance
        private static bool _online = true;

        // Represents connection to the local DB
        private static readonly Database LocalDb = new();

        // Represents connection to the remote (Azure) DB
        private static readonly Database RemoteDb = new();

        /// <summary>
        /// Syncs the remote (local) model with azure counterpart.
        /// Detects any conflicts bewtween the data models, and returns response according to the results.
        /// </summary>
        /// <param name="incomingModel">The model from local DB to be compared against Azure DB.</param>
        /// <param name="mode">Syncing from online to offline or from offline to online -> affects the syncing behaviour.</param>
        /// <returns>Response on any conflicts or OK response if no conflicts detected during sync.</returns>
        public static string Sync(SyncMode mode)
        {
            var resp = new SyncResponse
            {
                Status = Status.OK,
            };

            if (_online && mode == SyncMode.OfflineToOnline)
            {
                resp.Status = Status.INVALIDREQUEST;
                resp.Response = "ERROR: Cannot move Offline -> Online: Already online.";
            }

            if (!_online && mode == SyncMode.OnlineToOffline)
            {
                resp.Status = Status.INVALIDREQUEST;
                resp.Response = "ERROR: Cannot move Online -> Offline: Already offline.";
            }

            if (mode == SyncMode.OnlineToOffline)
            {
                // Wrapped in a transaction scope
                try
                {
                    LocalDb.Truncate(); // Truncate Local DB
                    LocalDb.CopyFrom(RemoteDb); // CopyFrom Remote DB into Local DB
                    // Commit();

                    _online = false; // Go To Offline
                    resp.Response = "SYNC OK.";
                }
                catch (Exception e)
                {
                    // Rollback();
                    Console.WriteLine(e);
                    throw;
                }
            }
            else if (mode == SyncMode.OfflineToOnline)
            {
                // Ensure we have a valid connection to remote DB
                if (!EnsureConnected())
                    throw new ApplicationException("ERROR: Connection failure. No access to remote DB.");

                // In real scenario, for each table, collect any changed ones
                // Check if any rows with same ID have collision

                // The ID of the model modified in both dbs
                const int id = 1;

                // Fetch the latest versions and check for conflicts
                var localLatest = LocalDb.Get(id);
                var remoteLatest = RemoteDb.Get(id);

                // TODO DETECT CHANGES BETWEEN DATABASES
                // Check if conflicts are possible

                // If both have same hash -> no changes were made in both
                if (localLatest.Hash == remoteLatest.Hash)
                {
                    // Nothing changed, do nothing and return sync ok
                    resp.Response = "SYNC OK. No changed made in either endpoints.";
                }

                // TODO elseif More checks to verify changes

                else
                {
                    // Ensure no conflicts between the latest models
                    resp = SyncService.Sync(localLatest, remoteLatest);

                    // If the merge was done ok,
                    // ensure the latest changes from local are inserted into cloud
                    if (resp.Status == Status.OK)
                    {
                        resp.Response = "SYNC OK. No conflicts detected. Latest change from local stored.";
                        RemoteDb.Upsert(localLatest);

                        // Back in online mode
                        _online = true;
                    }
                    else if (resp.Status != Status.CONFLICT)
                    {
                        resp.Response = "Unknown error occurred during sync.";
                    }
                    else
                    {
                        resp.Response =
                            "Conflicts detected during sync. Resolve them and call ResolveConflict(ResolutionDataModel)";
                    }
                }
            }

            return JsonConvert.SerializeObject(resp);
        }

        /// <summary>
        /// Returns the module that corresponds with the given ID.
        /// </summary>
        /// <returns></returns>
        public static string GetModuleData(int moduleId)
        {
            var model = _online ? RemoteDb.Get(moduleId) : LocalDb.Get(moduleId);

            var resp = new ApiResponse
            {
                Status = Status.OK,
                Response = JsonConvert.SerializeObject(model),
            };

            return JsonConvert.SerializeObject(resp);
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
            var result = InsertModule(JsonConvert.DeserializeObject<DataModel>(model));

            var resp = new ApiResponse
            {
                Status = Status.OK,
                Response = JsonConvert.SerializeObject(result),
            };

            return JsonConvert.SerializeObject(resp);
        }

        private static DataModel InsertModule(DataModel model)
        {
            var ins = _online ? RemoteDb.Upsert(model) : LocalDb.Upsert(model);
            return ins;
        }
    }
}