using ConflictManager.Backend.Models;
using JsonDiffPatchDotNet;
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
                IncomingModel = incomingModel,
            };

            if (remoteModel == null)
            {
                resp.Status = Status.NOTFOUND;
                return resp;
            }

            resp.CurrentModel = remoteModel;

            // Check the difference between the azure and incoming model datas
            var diff = CompareData(remoteModel.Data, incomingModel.Data);

            // If no conflicts, insert the incoming model into db
            if (diff == null)
            {
                // Return Sync OK
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
            return CompareData(localData == null ? JToken.Parse("{}") : JToken.Parse(localData), remoteData == null ? JToken.Parse("{}") : JToken.Parse(remoteData));
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
}