using System.Threading.Tasks;
using QX.Zoo.Accumulative;

namespace QX.Zoo.Tests.Animals
{
  public class MisteriousZoo
  {
    private readonly IAccumulatingFactory<Citizen> _accumulatingFactory;

    public MisteriousZoo(IAccumulatingFactory<Citizen> accumulatingFactory)
    {
      _accumulatingFactory = accumulatingFactory;
    }

    public async Task<long> AddCitizen(string citizenId, string address)
    {
      return await _accumulatingFactory.ApplyUpdateAsync(new CreateCitizen {CitizenId = citizenId, Address = address});
    }

    public async Task<long> AddElephant(string citizenId, string name, int age)
    {
      return await _accumulatingFactory.ApplyUpdateAsync(new CreateElephant {CitizenId = citizenId, Address = "Elephantia", Name = name, Age = age});
    }

    public async Task<long> MoveCitizen(long citizenVersion, string address)
    {
      return await _accumulatingFactory.ApplyUpdateAsync(new MoveCitizen {BaseVersionNumber = citizenVersion, NewAddress = address});
    }

    //public async Task<long> RenameElephant(long elephantVersion, string name)
    //{
    //  return await _accumulatingFactory.ApplyUpdateAsync(new ChangeElephantName {BaseVersionNumber = elephantVersion, Name = name});
    //}
  }
}
