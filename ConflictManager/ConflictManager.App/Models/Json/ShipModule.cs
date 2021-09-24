using ConflictManager.App.Services;

namespace ConflictManager.App.Models.Json
{
    public class ShipModule : Module<Ship>
    {
        public ShipModule(int id) : base(id)
        {
        }

        public override void DoSomethingWithTheData()
        {
            if (Data == null)
            {
                Data = StaticDataService.Ship1;
            }
            else
            {
                Data.Engines.Add(StaticDataService.De);
            }
        }
    }
}