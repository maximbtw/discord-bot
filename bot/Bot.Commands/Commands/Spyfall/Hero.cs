using System.Text.Json;
using System.Text.Json.Serialization;

namespace Bot.Commands.Commands.Spyfall;

internal class Hero
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("localized_name")]
    public string LocalizedName { get; set; } = string.Empty;

    [JsonPropertyName("primary_attr")]
    public PrimaryAttribute PrimaryAttribute { get; set; }

    [JsonPropertyName("attack_type")]
    public string AttackType { get; set; }= string.Empty;

    [JsonPropertyName("roles")] 
    public List<string> Roles { get; set; } = new();

    [JsonPropertyName("legs")]
    public int Legs { get; set; }
    
    public string Url
    {
        get
        {
            string urlHeroCode = this.LocalizedName.ToLower().Replace(" ", "");

            return $"https://www.dota2.com/hero/{urlHeroCode}";
        }
    }
    
    public string ImageUrl
    {
        get
        {
            string imageHeroCode = this.Name.ToLower().Replace("npc_dota_hero_", "").Replace(" ", "_").Replace("-", "");

            return $"https://cdn.steamstatic.com/apps/dota2/images/dota_react/heroes/{imageHeroCode}.png";
        }
    }
}

[JsonConverter(typeof(PrimaryAttributeConverter))]
internal enum PrimaryAttribute
{
   // [JsonPropertyName("str")]
    Strength,

  //  [JsonPropertyName("agi")]
    Agility,

 //   [JsonPropertyName("int")]
    Intelligence,

    //  [JsonPropertyName("all")]
    Universal
}


internal class PrimaryAttributeConverter : JsonConverter<PrimaryAttribute>
{
    public override PrimaryAttribute Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        string value = reader.GetString()!.ToLower();
        return value switch
        {
            "str" => PrimaryAttribute.Strength,
            "agi" => PrimaryAttribute.Agility,
            "int" => PrimaryAttribute.Intelligence,
            "all" => PrimaryAttribute.Universal,
            _ => throw new JsonException($"Неизвестный PrimaryAttribute: {value}")
        };
    }

    public override void Write(Utf8JsonWriter writer, PrimaryAttribute value, JsonSerializerOptions options)
    {
        // При сериализации записываем enum имя
        writer.WriteStringValue(value.ToString());
    }
}