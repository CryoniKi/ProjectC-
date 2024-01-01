using System.Reflection;
using System.Text.Json;

namespace Installer;
internal class Config(JsonElement element)
{
    public Config() : this(LoadConfig()) { }

    public string User { get; } = element.GetProperty(nameof(User)).GetString() ?? throw new KeyNotFoundException();
    public string Project { get; } = element.GetProperty(nameof(Project)).GetString() ?? throw new KeyNotFoundException();
    public string FileName { get; } = element.GetProperty(nameof(FileName)).GetString() ?? throw new KeyNotFoundException();
    public string ReleaseVersion { get; } = element.GetProperty(nameof(ReleaseVersion)).GetString() ?? throw new KeyNotFoundException();

    private static JsonElement LoadConfig()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = "Installer.appsettings.json";

        using Stream stream = assembly.GetManifestResourceStream(resourceName) ?? throw new NullReferenceException("Manifest was null");
        return JsonSerializer.Deserialize<JsonElement>(stream).GetProperty("Github");
    }

    public string GetGithubLink() => $"https://github.com/{User}/{Project}/releases/download/{ReleaseVersion}/{FileName}";
    public string GetCacheLocation() => $"Cache/{FileName}";
}
