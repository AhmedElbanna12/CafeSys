using System.Text.Json.Serialization;

namespace Foodics.Dtos.Paymob
{
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public class SourceData
    {
        public string pan { get; set; } = "";

        public string type { get; set; } = "";

        public string sub_type { get; set; } = "";
    }
}
