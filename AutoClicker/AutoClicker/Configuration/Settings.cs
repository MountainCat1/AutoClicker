using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AutoClicker.Configuration;

public class Settings
{
    public static Settings Singleton { get; private set; } = new Settings();
    
    public KeyBinds KeyBinds { get; set; } = new KeyBinds();

    public static void Load(string json)
    {
        var jsonSettings = new JsonSerializerOptions()
        {
            Converters = { new JsonStringEnumConverter() }
        };

        Singleton = JsonSerializer.Deserialize<Settings>(json, jsonSettings) ??
                    throw new SerializationException("Failed to deserialize settings");
    }
}