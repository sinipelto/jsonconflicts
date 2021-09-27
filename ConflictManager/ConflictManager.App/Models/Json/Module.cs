using Newtonsoft.Json;

namespace ConflictManager.App.Models.Json
{
    public abstract class Module<T>
    {
        private readonly int _id;

        protected T Data { get; set; }

        protected string Conflicts { get; set; }

        protected string ConflictKey { get; set; }

        protected Module(int id)
        {
            _id = id;
        }

        public void SetData(string data)
        {
            Data = JsonConvert.DeserializeObject<T>(data);
        }

        public string GetData()
        {
            return Data == null ? null : JsonConvert.SerializeObject(Data);
        }

        public void SetConflictData(string data, string key, string properties)
        {
            Conflicts = data;
            ConflictKey = key;

            HandleConflictValues(properties);
        }

        public abstract void DoSomethingWithTheData(int action);

        public void HandleConflictValues(string properties)
        {
        }
    }
}