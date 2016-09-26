using QX.Zoo.Hold;

namespace QX.Zoo.Tests.Animals.StoredAnimals
{
  public class StoredElephant : StoredDocument
  {
    public string CitizenId { get; set; }
    public string Address { get; set; }
    public string Name { get; set; }
    public int Age { get; set; }
    public bool IsPink { get; set; }

    public override string ToString()
    {
      return $"'{Name}' - the Mongo Elephant. {Age} years old, lives at {Address}, SSN #{CitizenId}{(IsPink?", PINK!!!":"")}";
    }
  }
}
