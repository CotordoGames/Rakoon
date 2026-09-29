using System.Text.Json.Serialization;

[JsonSourceGenerationOptions(PropertyNameCaseInsensitive = true, IncludeFields = true)]
[JsonSerializable(typeof(LevelMeta))]
[JsonSerializable(typeof(MusicMeta))]
[JsonSerializable(typeof(ConfigMeta))]
[JsonSerializable(typeof(string[]))]
[JsonSerializable(typeof(float[]))]
[JsonSerializable(typeof(int[]))]
public partial class AppJsonContext : JsonSerializerContext { }