using ConflictManager.App.Services;

namespace ConflictManager.App.Models.Json
{
    public class ShipModule : Module<Ship>
    {
        public ShipModule(int id) : base(id)
        {
        }

        public override void DoSomethingWithTheData(int action)
        {
            if (action == 1)
            {
                if (Data == null)
                {
                    Data = StaticDataService.Ship1;
                }
                else
                {
                    Data.Engines.Add(StaticDataService.Eng1);
                    Data.Engines.Add(StaticDataService.Te);
                }
            }
            else if (action == 2)
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
}