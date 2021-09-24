using Newtonsoft.Json;

namespace ConflictManager.App.Models.Json
{
    public abstract class Module<T>
    {
        private readonly int _id;

        protected T Data { get; set; }

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

        public abstract void DoSomethingWithTheData();
    }
}