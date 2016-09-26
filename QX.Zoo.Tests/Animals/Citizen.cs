using QX.Zoo.Accumulative;
using QX.Zoo.Hold;

namespace QX.Zoo.Tests.Animals
{
  public class Citizen : ICloneable<Citizen>, 
    ICumulativeUpdateAccumulator<Citizen, CreateCitizen>, 
    ICumulativeUpdateAccumulator<Citizen, MoveCitizen>
  {
    public string CitizenId { get; set; }
    public string Address { get; set; }

    public void CopyFrom(Citizen source)
    {
      CitizenId = source.CitizenId;
      Address = source.Address;
    }

    public void ApplyUpdate(CreateCitizen update)
    {
      CitizenId = update.CitizenId;
      Address = update.Address;
    }

    public void ApplyUpdate(MoveCitizen update)
    {
      Address = update.NewAddress;
    }
  }
}