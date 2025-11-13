using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
namespace HolidayApi.Domain
{
    public class Holiday
    {
        [Key]
        public int Id { get; set; }

        [JsonPropertyName("date")]
        public DateTime Date { get; set; }

        [JsonPropertyName("localName")]
        public string LocalName { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("countryCode")]
        public string CountryCode { get; set; } = string.Empty;

        [JsonPropertyName("fixed")]
        public bool Fixed { get; set; }

        [JsonPropertyName("global")]
        public bool Global { get; set; }

        [JsonPropertyName("counties")]
        public List<string>? Counties { get; set; }

        [JsonPropertyName("launchYear")]
        public int? LaunchYear { get; set; }

        [JsonPropertyName("types")]
        public List<string> Types { get; set; } = new List<string>();
    }
}
