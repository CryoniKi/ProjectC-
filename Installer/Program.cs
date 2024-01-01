using Installer;
using Spectre.Console;
using System.Runtime.InteropServices.ComTypes;
using System.IO.Compression;
using static System.Environment;

var config = new Config();
var httpClient = new HttpClient();

string tempPath = Path.Combine(Path.GetTempPath(), "SplatoonLoadoutInstaller");
string tempFile = Path.Combine(tempPath, config.FileName);
string programPath = Path.Combine(GetFolderPath(SpecialFolder.LocalApplicationData), config.Project);

DirectoryExtensions.ExistOrCreate(tempPath);

await AnsiConsole.Status()
    .Spinner(Spinner.Known.Dots)
    .StartAsync("Downloading Project C-", async ctx => {
        await Download(ctx);
        Unpack(ctx);
    });

if (AnsiConsole.Confirm("Do you want to create a Start Menu shortcut?", true)) {
    try {
        string startMenuPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu), "Programs");
        WriteShortcut(startMenuPath);
    }
    catch (Exception ex) {
        WriteErrorAndDie(ex);
    }
}

if (AnsiConsole.Confirm("Do you want to create a Desktop shortcut?", true)) {
    try {
        string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
        WriteShortcut(desktopPath);
    }
    catch (Exception ex) {
        WriteErrorAndDie(ex);
    }
}

//Remove Cache folder
try {
    new DirectoryInfo(tempPath).Delete(true);
}
catch (Exception ex) {
    WriteErrorAndDie(ex);
}
async Task Download(StatusContext ctx)
{
    try {
        HttpResponseMessage response = await httpClient.GetAsync(config.GetGithubLink());
        response.EnsureSuccessStatusCode();
        using var fs = new FileStream(tempFile, FileMode.OpenOrCreate);
        await response.Content.CopyToAsync(fs);

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
        
        bool existed = DirectoryExtensions.ExistOrCreate(programPath);

        if (existed) {
            AnsiConsole.WriteLine("[LOG] Removing old files");
            DirectoryInfo di = new DirectoryInfo(programPath);
            foreach (FileInfo file in di.GetFiles()) { file.Delete(); }
            foreach (DirectoryInfo dir in di.GetDirectories()) { dir.Delete(true); }
        }

        ZipFile.ExtractToDirectory(tempFile, programPath);
        AnsiConsole.WriteLine("[LOG] Files unpacked");
    }
    catch(Exception ex) {
        WriteErrorAndDie(ex);
    }
}

void WriteErrorAndDie(Exception ex)
{
    AnsiConsole.WriteException(ex, ExceptionFormats.ShortenEverything);
    Exit(1);
}

void WriteShortcut(string destination)
{
    IShellLink link = (IShellLink)new ShellLink();

    link.SetDescription("Splatoon Loadout");
    link.SetPath(Path.Combine(programPath, "SplatoonLoadout.exe"));
    link.SetIconLocation(Path.Combine(programPath, "SplatoonLoadout.exe"), 0);

    IPersistFile file = (IPersistFile)link;
    file.Save(Path.Combine(destination, "Splatoon Loadout.lnk"), false);
}