using Newtonsoft.Json;

namespace OneTon.PersistentData
{
    [System.Serializable]
    public class StringHashPair
    {
        [JsonConstructor]
        public StringHashPair(string @string, string hash)
        {
            String = @string;
            Hash = hash;
        }

        public StringHashPair(string dataString)
        {
            String = dataString;
            Hash = PersistentDataService.ComputeHash(dataString);
        }

        public string String { get; private set; }
        public string Hash { get; private set; }
    }
}