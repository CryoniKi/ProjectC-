using Microsoft.Extensions.Configuration;
using System.Diagnostics;
using System.Reflection;

namespace Installer;
internal class Config
{
    private readonly IConfiguration _configuration = LoadConfig();
    private static IConfiguration LoadConfig()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = "Installer.appsettings.json"; // Update the namespace accordingly

        using (Stream stream = assembly.GetManifestResourceStream(resourceName)) {
            using (StreamReader reader = new StreamReader(stream)) {
                var json = reader.ReadToEnd();
                return new ConfigurationBuilder()
                    .AddJsonStream(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(json)))
                    .Build();
            }
        }
    }
    
    public string GetUser() => _configuration["Github:User"];
    public string GetProject() => _configuration["Github:Project"];
    public string GetReleaseVersion() => _configuration["Github:ReleaseVersion"];
    public string GetFileName() => _configuration["Github:FileName"];

    public string GetGithubLink() => $"https://github.com/{GetUser()}/{GetProject()}/releases/download/{GetReleaseVersion()}/{GetFileName()}";
    public string GetCacheLocation() => $"Cache/{_configuration["Github:FileName"]}";
}
