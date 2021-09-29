using ConflictManager.Backend.Models;
using JsonDiffPatchDotNet;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace ConflictManager.Backend.Services
{
    internal static class SyncService
    {
        public static SyncResponse Sync(DataModel incomingModel, DataModel remoteModel)
        {
            Debug.Assert(incomingModel.ModuleId != null, "incomingModel.ModuleId != null");

            var resp = new SyncResponse
            {
                Status = Status.OK,
                IncomingModel = incomingModel.ToString(), // TODO BROKEN
            };

            if (remoteModel == null)
            {
                resp.Status = Status.NOTFOUND;
                return resp;
            }

            resp.OriginalModel = remoteModel.ToString(); // TODO BROKEN

            // Check the difference between the azure and incoming model datas
            var diff = CompareData(remoteModel.Data, incomingModel.Data);

            // If no conflicts, insert the incoming model into db
            if (diff == null)
            {
                // Return Sync OK
                return resp;
            }

            // No conflicts - changes have occurred only locally
            if (diff.Conflicts.Count <= 0 && diff.AzureOnlyProperties.Count <= 0)
            {
                return resp;
            }

            resp.Status = Status.CONFLICT;
            //resp.DifferenceDetails = diff; // TODO BROKEN

            return resp;
        }

        public static string GetEffDiff(string localData, string remoteData)
        {
            var tool = new JsonDiffPatch(new Options
            {
                ArrayDiff = ArrayDiffMode.Efficient,
                TextDiff = TextDiffMode.Simple,
                MinEfficientTextDiffLength = 0,
            });

            var localJson = localData == null ? new JObject() : JToken.Parse(localData);
            var remoteJson = remoteData == null ? new JObject() : JToken.Parse(remoteData);

            return GetDiff(localJson, remoteJson, tool)
                .ToString(Formatting.None);
        }

        private static JToken GetDiff(JToken localData, JToken remoteData, JsonDiffPatch tool)
        {
            return tool.Diff(localData, remoteData);
        }

        /// <summary>
        /// String overload for the parser.
        /// </summary>
        /// <param name="localData">First JSON object as string.</param>
        /// <param name="remoteData">Second JSON object as string.</param>
        /// <returns>NULL if there aren't any differences. Otherwise returns the differencing parts between the two JSON objects.</returns>
        private static DifferenceDetails CompareData(string localData, string remoteData)
        {
            return CompareData(localData == null ? JToken.Parse("{}") : JToken.Parse(localData), remoteData == null ? JToken.Parse("{}") : JToken.Parse(remoteData), new Options
            {
                ArrayDiff = ArrayDiffMode.Simple,
                TextDiff = TextDiffMode.Simple,
                MinEfficientTextDiffLength = 0,
            });
        }

        /// <summary>
        /// Checks if the compared JSON values have any differences
        /// Returns the differencing properties with the differing parts.
        /// </summary>
        /// <param name="localDbData">First JSON object</param>
        /// <param name="azureDbData">Second JSON object</param>
        /// <returns>NULL if there aren't any differences. Otherwise returns the differencing parts between the two JSON objects.</returns>
        private static DifferenceDetails CompareData(JToken localDbData, JToken azureDbData, Options opts)
        {
            var tool = new JsonDiffPatch(opts);

            var diff = GetDiff(localDbData, azureDbData, tool);

            // diff != null -> diff(s) exist
            if (diff == null) return null;

            // Construct response object
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
            //Console.WriteLine($"\r\nDIFF PROPS:\r\n{(string.Join("\r\n", diffs))}");

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
                    Console.WriteLine("ERROR: WENT ELSE -> diff case unhandled.");
                    Console.Error.WriteLine("ERROR: WENT ELSE -> diff case unhandled.");
                }
            }
        }
    }
}