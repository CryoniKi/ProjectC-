using System.Net.Http.Json;
using System.Reflection;
using System.Text.Json;

namespace SplatoonLoadout.Services;
internal class UpdateChecker(IHttpClientFactory factory)
{
    private const string REPO_URL = "https://api.github.com/repos/CryoniKi/ProjectC-/releases";
    private readonly IHttpClientFactory _factory = factory;

    private static Version GetCurrentVersion() => Version.Parse(Assembly.GetExecutingAssembly().GetName().Version!.ToString());
    private async Task<(Version version, string url)> GetRemoteVersion()
    {
        try {
            using var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Add("User-Agent", "SplatoonLoadout");

            var element = await client.GetFromJsonAsync<JsonElement>(REPO_URL);
            var latest = element.EnumerateArray().First();
            var version = latest.GetProperty("tag_name").GetString() ?? string.Empty;
            return (Version.Parse(version[1..]), version);
        }
        catch {
            return (Version.Parse("0.0.0"), "V0.0.0");
        }
        
    }
    public async Task<(bool,string)> CheckForUpdate()
    {
        var current = GetCurrentVersion();
        var (remote, tag) = await GetRemoteVersion();

        return (remote > current, tag);
    }
}
