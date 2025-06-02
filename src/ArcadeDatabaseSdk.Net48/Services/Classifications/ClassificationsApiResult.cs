#nullable enable
using Newtonsoft.Json;
namespace ArcadeDatabaseSdk.Net48.Services.Categories;

public partial class ClassificationsApiResult
{
    [JsonProperty("id")]
    public int Id { get; set; }

    [JsonProperty("code")]
    public string? Code { get; set; }

    [JsonProperty("description")]
    public string? Description { get; set; }
}
