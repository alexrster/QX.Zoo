#if NETFRAMEWORK && false
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using QX.Zoo.Hold;
using QX.Zoo.Hold.Mongo;
using QX.Zoo.Tests.Animals.StoredAnimals;
using System.Threading.Tasks;
using Xunit;

namespace QX.Zoo.Tests
{
  public class MongoTest
  {
    public IConfiguration Configuration { get; set; }
    public ILogger Log { get; set; }

    [Fact]
    public async Task BaseCreateUpdate_Success()
    {
      Log.LogInformation("Initialize MongoDB");
      MongoDocumentStoreConfiguration.ConfigureStoredDocumentMappings<StoredDocument>(cm => cm.MapIdProperty(x => x.RecordId).SetElementName("_id"));
      MongoDocumentStoreConfiguration.ConfigureStoredDocumentMappings<StoredElephant>();

      var documentStore = MongoDocumentStore<StoredElephant, string>.Create(x => x.CitizenId, @"mongodb://localhost:27017/zoo", "citizens");
      Log.LogVerbose("Connected to MongoDB");

      var elephant = new StoredElephant
      {
        Name = "Muter",
        Address = "Underground",
        CitizenId = "AE001",
        Age = 88,
        IsPink = false
      };

      Log.LogVerbose($"Put new Elephant to Store\r\n{elephant}");
      var recordId = await documentStore.CreateAsync(elephant);
      Log.LogVerbose($"Inserted record id '{recordId}'");

      Log.LogVerbose($"Get Elephant filtering by Key eq '{elephant.CitizenId}' from Store");
      var muterElephantInfo = (await documentStore.FindAsync(x => x.CitizenId == elephant.CitizenId)).FirstOrDefault();
      var muterElephant = muterElephantInfo.Value;
      Log.LogVerbose($"Store returned record '{muterElephantInfo.Key}': \"{muterElephant}\"");

      muterElephant.Age++;
      Log.LogVerbose($"Put updated Elephant to Store\r\n{muterElephant}");
      recordId = await documentStore.UpdateAsync(muterElephant);
      Log.LogVerbose($"Inserted record id '{recordId}'");

      Log.LogVerbose($"Get Elephant by record id eq '{recordId}' from Store");
      var agedMuterElephant = await documentStore.GetAsync(recordId);
      Log.LogVerbose($"Store returns: \"{agedMuterElephant}\"");

      agedMuterElephant.IsPink = true;
      Log.LogVerbose($"Put updated Elephant to Store\r\n{agedMuterElephant}");
      recordId = await documentStore.UpdateAsync(agedMuterElephant);
      Log.LogVerbose($"Inserted record id '{recordId}'");

      Log.LogVerbose($"Get Elephant by key eq '{agedMuterElephant.CitizenId}' from Store");
      var happyMuterElephantInfo = await documentStore.GetAsync(agedMuterElephant.CitizenId);
      Log.LogVerbose($"Store returned '{happyMuterElephantInfo.Key}': \"{happyMuterElephantInfo.Value}\"");
    }
  }
}
#endif
