using ConflictManager.App.Models.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ConflictManager.App.Services
{
    public static class RandomJsonService
    {
        private static readonly Random Random = new();

        private static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[Random.Next(s.Length)]).ToArray());
        }

        private static readonly List<string> Properties = new()
        {
            "Temp",
            "CoolantTemp",
            "ParentNode",
            "ChildNodes",
            "Value",
            "Description",
            "Values"
        };

        private static bool IsInt => Random.NextDouble() <= 0.5; // 50% chance

        private static bool IsArr => Random.NextDouble() <= 0.5; // 50% chance

        private static bool IsObj => Random.NextDouble() <= 0.5; // 50% chance

        public static DataModel GenerateModel()
        {
            var data = "{";

            var indices = Enumerable.Range(0, Properties.Count).OrderBy(i => Guid.NewGuid()).ToList();

            var frst = true;

            foreach (var index in indices.Take(Random.Next(1, Properties.Count)))
            {
                if (!frst)
                {
                    data += ",";
                }

                if (IsObj)
                {
                    data += "\"" + RandomString(Random.Next(3, 15)) + "\":";
                    data += "{";

                    var indc = Enumerable.Range(0, Properties.Count - 1).OrderBy(i => Guid.NewGuid()).ToList();

                    var firstX = true;

                    foreach (var idx in indc.Take(Random.Next(1, Properties.Count)))
                    {
                        if (!firstX)
                        {
                            data += ",";
                        }

                        data += "\"" + Properties[idx] + "\":" + (IsInt ? Random.Next(0, 99999) : "\"" + RandomString(Random.Next(3, 15)) + "\"");

                        firstX = false;
                    }

                    data += "}";
                }
                else if (IsArr)
                {
                    data += "\"" + Properties[index] + "\":[";

                    var first = true;

                    for (var i = 0; i < Random.Next(1, 6); i++)
                    {
                        if (!first)
                        {
                            data += ",";
                        }

                        data += (IsInt ? Random.Next(0, 99999) : "\"" + RandomString(Random.Next(3, 15)) + "\"");

                        first = false;
                    }

                    data += "]";
                }
                else if (IsInt)
                {
                    data += "\"" + Properties[index] + "\":" + Random.Next(0, 99999);
                }
                else
                {
                    data += "\"" + Properties[index] + "\":" + "\"" + RandomString(Random.Next(3, 15)) + "\"";
                }

                frst = false;
            }

            data += "}";

            return new DataModel
            {
                Created = DateTime.UtcNow,
                Modified = DateTime.UtcNow,
                Version = Random.Next(0, 10),
                Data = data,
            };
        }
    }
}