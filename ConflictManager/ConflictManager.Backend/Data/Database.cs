using ConflictManager.Backend.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using static System.Int32;

namespace ConflictManager.Backend.Data
{
    internal class Database
    {
        public IEnumerable<DataModel> GetAll() => _data.AsEnumerable();

        public void CopyFrom(Database source)
        {
            foreach (var val in source._data)
            {
                var model = new DataModel
                {
                    ModuleId = val.ModuleId,
                    Hash = val.Hash,
                    Created = val.Created,
                    Modified = val.Modified,
                    Version = val.Version,
                    Data = val.Data,
                };

                _data.Add(model);
            }
        }

        public DataModel Get(int id, int? version = null)
        {
            return
                version == null
                ? _data.Where(i => i.ModuleId == id).OrderByDescending(i => i.Version).FirstOrDefault()
                : _data.Where(i => i.ModuleId == id).FirstOrDefault(i => i.Version == version);
        }

        public void InsertMany(IEnumerable<DataModel> models)
        {
            foreach (var dataModel in models.OrderBy(i => i.Version))
            {
                Upsert(dataModel);
            }
        }

        public DataModel Upsert(DataModel model)
        {
            if (model.ModuleId == null) throw new ArgumentNullException();

            var val = _data.Where(i => i.ModuleId == model.ModuleId).OrderByDescending(i => i.Version).FirstOrDefault();

            // Greatest ID or 0
            var idMax = _data.Max(i => i.ModuleId) ?? 0;

            var fresh = new DataModel
            {
                Modified = DateTime.UtcNow,
                Data = model.Data,
            };

            // Module with that ID does not exist
            if (val == null)
            {
                fresh.ModuleId = idMax + 1;
                fresh.Created = DateTime.UtcNow;
                fresh.Version = 0;
            }
            // Module exists, new version
            else
            {
                fresh.ModuleId = val.ModuleId;
                fresh.Created = val.Created;
                fresh.Version = val.Version + 1;
            }

            fresh.Hash = fresh.GetHashCode();
            _data.Add(fresh);

            return fresh;
        }

        public void Truncate() => _data.Clear();

        private readonly Collection<DataModel> _data = new()
        {
            new DataModel
            {
                ModuleId = 2,
                Hash = new Random().Next(MinValue, MaxValue),
                Created = DateTime.UtcNow,
                Modified = DateTime.UtcNow,
                Version = 0,
                Data = null,
            },
            new DataModel
            {
                ModuleId = 1,
                Hash = new Random().Next(MinValue, MaxValue),
                Created = DateTime.UtcNow,
                Modified = DateTime.UtcNow,
                Version = 0,
                Data = null,
            },
            new DataModel
            {
                ModuleId = 1,
                Hash = new Random().Next(MinValue, MaxValue),
                Created = DateTime.UtcNow,
                Modified = DateTime.UtcNow,
                Version = 1,
                Data = null,
            },
        };
    }
}