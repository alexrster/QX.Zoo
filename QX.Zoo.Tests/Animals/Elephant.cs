using QX.Zoo.Accumulative;
using QX.Zoo.Accumulative.Base;
using QX.Zoo.Hold;

namespace QX.Zoo.Tests.Animals
{
  public class Elephant : Citizen, ICloneable<Elephant>,
    ICumulativeUpdateAccumulator<Elephant, MakeElephantCool>, 
    ICumulativeUpdateAccumulator<Elephant, CreateElephant>, 
    ICumulativeUpdateAccumulator<Elephant, ChangeElephantName>, 
    ICumulativeUpdateAccumulator<Elephant, MakeElephantGrow>, 
    ICumulativeUpdateAccumulator<Elephant, MakeElephantHappy>
  {
    public string Name { get; private set; }
    public int Age { get; private set; }
    public bool IsPink { get; private set; }

    public void CopyFrom(Elephant source)  
    {
      base.CopyFrom(source);

      Name = source.Name;
      Age = source.Age;
      IsPink = source.IsPink;
    }

    public void ApplyUpdate(MakeElephantCool update)
    {
      IsPink = true;
    }

    public void ApplyUpdate(CreateElephant update)
    {
      base.ApplyUpdate(update);

      Name = update.Name;
      Age = update.Age;
    }

    public void ApplyUpdate(ChangeElephantName update)
    {
      Name = update.Name;
    }

    public void ApplyUpdate(MakeElephantGrow update)
    {
      Age = update.Age;
    }

    public void ApplyUpdate(MakeElephantHappy update)
    {
      Age = 25;
    }
  }

  public class CreateCitizen : CumulativeUpdate<Citizen, CreateCitizen>
  {
    public string CitizenId { get; set; }
    public string Address { get; set; }
  }

  public class MoveCitizen : CumulativeUpdate<Citizen, MoveCitizen>
  {
    public string NewAddress { get; set; }
  }

  public class MakeElephantCool : CumulativeUpdate<Elephant, MakeElephantCool>
  { }

  public class CreateElephant : CreateCitizen, ICumulativeUpdate<Elephant, CreateElephant>
  {
    public string Name { get; set; }
    public int Age { get; set; }

    public void Apply(Elephant obj)
    {
      obj.ApplyUpdate(this);
    }
  }

  public class ChangeElephantName : CumulativeUpdate<Elephant, ChangeElephantName>
  {
    public string Name { get; set; }
  }

  public class MakeElephantGrow : CumulativeUpdate<Elephant, MakeElephantGrow>
  {
    public int Age { get; set; }
  }

  public class MakeElephantHappy : CumulativeUpdate<Elephant, MakeElephantHappy>
  { }
}
