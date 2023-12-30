using Installer;
using Spectre.Console;
using System.Runtime.InteropServices.ComTypes;
using System.IO.Compression;
using static System.Environment;

var config = new Config();
var httpClient = new HttpClient();

if (!Directory.Exists("Cache")) {
    Directory.CreateDirectory("Cache");
}

await AnsiConsole.Status()
    .Spinner(Spinner.Known.Dots)
    .StartAsync("Downloading Project C-", async ctx => {
        await Download(ctx);
        Unpack(ctx);
    });

if (AnsiConsole.Confirm("Do you want to create a Start Menu shortcut?", true)) {
    IShellLink link = (IShellLink)new ShellLink();
    link.SetDescription("Splatoon Loadout");
    var programPath = Path.Combine(GetFolderPath(SpecialFolder.LocalApplicationData), config.GetProject());
    link.SetPath(Path.Combine(programPath, "SplatoonLoadout.exe"));
    link.SetIconLocation(Path.Combine(programPath, "SplatoonLoadout.exe"), 0);

    IPersistFile file = (IPersistFile)link;
    string startMenuPath = Environment.GetFolderPath(Environment.SpecialFolder.StartMenu);
    file.Save(Path.Combine(startMenuPath, "Splatoon Loadout.lnk"), false);
}

if (AnsiConsole.Confirm("Do you want to create a Desktop shortcut?", true)) {
    IShellLink link = (IShellLink)new ShellLink();
    link.SetDescription("Splatoon Loadout");
    var programPath = Path.Combine(GetFolderPath(SpecialFolder.LocalApplicationData), config.GetProject());
    link.SetPath(Path.Combine(programPath, "SplatoonLoadout.exe"));
    link.SetIconLocation(Path.Combine(programPath, "SplatoonLoadout.exe"), 0);

    IPersistFile file = (IPersistFile)link;
    string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
    file.Save(Path.Combine(desktopPath, "Splatoon Loadout.lnk"), false);
}

async Task Download(StatusContext ctx)
{
    HttpResponseMessage response = await httpClient.GetAsync(config.GetGithubLink());
    using (var fs = new FileStream(config.GetCacheLocation(), FileMode.OpenOrCreate)) {
        await response.Content.CopyToAsync(fs);
    }

    AnsiConsole.WriteLine("[LOG] Downloaded");
}

void Unpack(StatusContext ctx)
{
    ctx.Status("Unpacking files");
    var programPath = Path.Combine(GetFolderPath(SpecialFolder.LocalApplicationData), config.GetProject());

    if (!Directory.Exists(programPath)) {
        Directory.CreateDirectory(programPath);
    }

    string[] files = Directory.GetFiles(programPath); 
    string[] directories = Directory.GetDirectories(programPath);
    if (files.Length > 0 || directories.Length > 0) {
        AnsiConsole.WriteLine("[LOG] Removing old files");
        DirectoryInfo di = new DirectoryInfo(programPath);
        foreach (FileInfo file in di.GetFiles()) { file.Delete(); }
        foreach (DirectoryInfo dir in di.GetDirectories()) { dir.Delete(true); }
    }

    ZipFile.ExtractToDirectory(config.GetCacheLocation(), programPath);
    AnsiConsole.WriteLine("[LOG] Unpacked");
}