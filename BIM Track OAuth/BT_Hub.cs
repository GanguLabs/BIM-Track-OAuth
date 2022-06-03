using Newtonsoft.Json;
using System;

namespace BIM_Track_OAuth
{
    public class BT_Hub
    {
        [JsonProperty("Id")]
        public string Id { get; set; }

        [JsonProperty("Name")]
        public string Name { get; set; }

        [JsonProperty("Author")]
        public Author Author { get; set; }

        [JsonProperty("Image")]
        public Image Image { get; set; }

        [JsonProperty("CreationDate")]
        public DateTimeOffset CreationDate { get; set; }

        [JsonProperty("RegionSummary")]
        public RegionSummary RegionSummary { get; set; }
    }

    public class Author
    {
        [JsonProperty("Id")]
        public long Id { get; set; }

        [JsonProperty("UserName")]
        public string UserName { get; set; }

        [JsonProperty("Email")]
        public string Email { get; set; }

        [JsonProperty("FirstName")]
        public string FirstName { get; set; }

        [JsonProperty("LastName")]
        public string LastName { get; set; }

        [JsonProperty("AvatarUrl")]
        public string AvatarUrl { get; set; }
    }

    public class Image
    {
        [JsonProperty("ThumbnailUrl")]
        public string ThumbnailUrl { get; set; }

        [JsonProperty("ThumbnailUrlExpiration")]
        public DateTime? ThumbnailUrlExpiration { get; set; }

        [JsonProperty("Id")]
        public long Id { get; set; }

        [JsonProperty("Name")]
        public string Name { get; set; }

        [JsonProperty("Size")]
        public long Size { get; set; }

        [JsonProperty("Date")]
        public DateTime? Date { get; set; }

        [JsonProperty("Url")]
        public string Url { get; set; }

        [JsonProperty("UrlExpiration")]
        public DateTime? UrlExpiration { get; set; }
    }

    public class RegionSummary
    {
        [JsonProperty("Location")]
        public string Location { get; set; }

        [JsonProperty("ApiRegionalUrl")]
        public string ApiRegionalUrl { get; set; }
    }
}
