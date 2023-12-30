using Installer;
using Spectre.Console;
using System.Runtime.InteropServices.ComTypes;
using System.IO.Compression;
using static System.Environment;

var config = new Config();
var httpClient = new HttpClient();

string tempPath = Path.Combine(Path.GetTempPath(), "SplatoonLoadoutInstaller");
string tempFile = Path.Combine(tempPath, config.GetFileName());
if (!Directory.Exists(tempPath)) {
    Directory.CreateDirectory(tempPath);
}

await AnsiConsole.Status()
    .Spinner(Spinner.Known.Dots)
    .StartAsync("Downloading Project C-", async ctx => {
        await Download(ctx);
        Unpack(ctx);
    });

if (AnsiConsole.Confirm("Do you want to create a Start Menu shortcut?", true)) {
    try {
        IShellLink link = (IShellLink)new ShellLink();
        link.SetDescription("Splatoon Loadout");
        var programPath = Path.Combine(GetFolderPath(SpecialFolder.LocalApplicationData), config.GetProject());
        link.SetPath(Path.Combine(programPath, "SplatoonLoadout.exe"));
        link.SetIconLocation(Path.Combine(programPath, "SplatoonLoadout.exe"), 0);

        IPersistFile file = (IPersistFile)link;
        string startMenuPath = Environment.GetFolderPath(Environment.SpecialFolder.StartMenu);
        file.Save(Path.Combine(startMenuPath, "Splatoon Loadout.lnk"), false);
    }
    catch (Exception ex) {
        WriteErrorAndDie(ex);
    }
}

if (AnsiConsole.Confirm("Do you want to create a Desktop shortcut?", true)) {
    try {
        IShellLink link = (IShellLink)new ShellLink();
        link.SetDescription("Splatoon Loadout");
        var programPath = Path.Combine(GetFolderPath(SpecialFolder.LocalApplicationData), config.GetProject());
        link.SetPath(Path.Combine(programPath, "SplatoonLoadout.exe"));
        link.SetIconLocation(Path.Combine(programPath, "SplatoonLoadout.exe"), 0);

        IPersistFile file = (IPersistFile)link;
        string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
        file.Save(Path.Combine(desktopPath, "Splatoon Loadout.lnk"), false);
    }
    catch (Exception ex) {
        WriteErrorAndDie(ex);

    }
}

//Remove Cache folder
try {
    DirectoryInfo di = new DirectoryInfo(tempPath);
    di.Delete(true);
}
catch (Exception ex) {
    WriteErrorAndDie(ex);
}
async Task Download(StatusContext ctx)
{
    try {
        HttpResponseMessage response = await httpClient.GetAsync(config.GetGithubLink());
        response.EnsureSuccessStatusCode();
        using (var fs = new FileStream(tempFile, FileMode.OpenOrCreate)) {
            await response.Content.CopyToAsync(fs);
        }

        AnsiConsole.WriteLine("[LOG] Downloaded");
    }
    catch(Exception ex) {
        WriteErrorAndDie(ex);
    }
    
}

void Unpack(StatusContext ctx)
{
    try {
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

        ZipFile.ExtractToDirectory(tempFile, programPath);
        AnsiConsole.WriteLine("[LOG] Unpacked");
    }
    catch(Exception ex) {
        WriteErrorAndDie(ex);
    }
}

void WriteErrorAndDie(Exception ex)
{
    AnsiConsole.WriteException(ex, ExceptionFormats.ShortenPaths | ExceptionFormats.ShortenTypes | ExceptionFormats.ShortenMethods | ExceptionFormats.ShowLinks);
    Environment.Exit(1);
}