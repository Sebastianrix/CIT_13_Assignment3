using System.Text.Json.Serialization;

public class Category
{
    [JsonPropertyName("cid")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }
}