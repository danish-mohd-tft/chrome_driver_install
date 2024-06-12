// See https://aka.ms/new-console-template for more information
using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.IO.Compression;
using System.Net;

Console.WriteLine("Hello, World!");
static string GetChromeVersion()
{
    //Log.LogMessage($"finding chromepath");
    string chromePath = FindChromePath();
    //Log.LogMessage($"got the chrome path");
    if (!string.IsNullOrEmpty(chromePath))
    {
        var versionInfo = FileVersionInfo.GetVersionInfo(chromePath);
        return versionInfo.FileVersion;
    }
    return null;
}


static string FindChromePath()
{
    // Registry keys for Chrome
    string[] regKeys = new string[]
    {
        @"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\chrome.exe",
        @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Google Chrome",
        @"SOFTWARE\Google\Chrome",
        @"SOFTWARE\Wow6432Node\Google\Chrome"
    };

    // Check both 64-bit and 32-bit registry views
    RegistryView[] registryViews = new RegistryView[]
    {
        RegistryView.Registry64,
        RegistryView.Registry32
    };

    // Try to find Chrome path from registry
    foreach (var view in registryViews)
    {
        foreach (string regKey in regKeys)
        {
            using (var baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, view))
            using (var key = baseKey.OpenSubKey(regKey))
            {
                if (key != null)
                {
                    var path = key.GetValue("InstallLocation") as string;
                    if (!string.IsNullOrEmpty(path))
                    {
                        string chromeExePath = Path.Combine(path, "chrome.exe");
                        if (File.Exists(chromeExePath))
                        {
                            return chromeExePath;
                        }
                    }
                }
            }
        }
    }

    return null;
}

static void DownloadChromeDriver(string version, string downloadPath)
{
    //Log.LogMessage($"starting download chromdriver");
    //Log.LogMessage($"downloadPath {downloadPath}");
    //string downloadUrl = $"https://chromedriver.storage.googleapis.com/{version}/chromedriver_win32.zip";
    string chromeaddreess = System.IO.Path.Combine(downloadPath, "chromedriver-win64");
    //Log.LogMessage($"chromeaddreess {chromeaddreess}");
    string[] parts = version.Split('.');

    // Extract the first part (major version)
    string majorVersion = parts[0];
    var finalver = "";

    using (WebClient client = new WebClient())
    {
        try
        {
            string url = "https://googlechromelabs.github.io/chrome-for-testing/known-good-versions.json";
            string response = client.DownloadString(url);
            var responseObject = JObject.Parse(response);


            // Get versions array
            var versions = responseObject["versions"];
            foreach (var version1 in versions)
            {
                string versionString = version1["version"].ToString();
                if (versionString.StartsWith(majorVersion))
                {
                    finalver = versionString;
                }
            }

        }
        catch (WebException ex)
        {
            //Log.LogMessage(ex.Message);
            throw ex;
        }
    }

    // If directory does not exist, don't even try   
    if (Directory.Exists(chromeaddreess))
    {
        Directory.Delete(chromeaddreess, true);
        //Directory.Delete(chromeaddreess);
    }
    string downloadUrl = $"https://storage.googleapis.com/chrome-for-testing-public/{finalver}/win64/chromedriver-win64.zip";
    //Log.LogMessage($"downloadUrl : {downloadUrl}");
    using (var client = new WebClient())
    {
        client.DownloadFile(downloadUrl, Path.Combine(downloadPath, "chromedriver_win64.zip"));

    }
    //Log.LogMessage($"Path.Combine(downloadPath, \"chromedriver_win64.zip\") : {Path.Combine(downloadPath, "chromedriver_win64.zip")}");
    ZipFile.ExtractToDirectory(Path.Combine(downloadPath, "chromedriver_win64.zip"), chromeaddreess);
    //Log.LogMessage($"completed download");
}
try
{
    // Log.LogMessage($"starting executioin chromdriver");
    //var path = AppDirectoryInfo.currentAssemblyLoadDirectoryForProcess();
    var path = "C:\\Program Files (x86)\\Think Future Technologies\\TFT_Bot_Runner";
    string chromedriverPath = "";
    string applicaitonchromeversionversion = "";
    string finalpath = path;
    string destinationPath = "";
    //Log.LogMessage($"calling get version issue");
    string chromeVersion = GetChromeVersion();
    string systemchromeversion = chromeVersion.Split('.')[0];

    string existingversion = "";
    if (File.Exists(System.IO.Path.Combine(path, "chromedriver.exe")))
    {
        using (Process process = new Process())
        {
            process.StartInfo.FileName = System.IO.Path.Combine(path, "chromedriver.exe");
            process.StartInfo.Arguments = "--version";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.CreateNoWindow = true;
            process.Start();

            existingversion = process.StandardOutput.ReadToEnd();

            process.WaitForExit();

        }
        int startIndex = existingversion.IndexOf(' ') + 1;

        // Find the index of the next space character after the version number
        int endIndex = existingversion.IndexOf('.', startIndex);

        // Extract the substring containing the version number
        applicaitonchromeversionversion = existingversion.Substring(startIndex, endIndex - startIndex);
    }


    if (applicaitonchromeversionversion != systemchromeversion)
    {
       // Log.LogMessage($"version difference hence downloading new");
        string chromedriverInstallPath = path;
        DownloadChromeDriver(chromeVersion, chromedriverInstallPath);
        //Log.LogMessage($"downloaded chrome driver");
        chromedriverPath = Path.Combine(chromedriverInstallPath, "chromedriver_win64.zip");

        using (ZipArchive archive = ZipFile.OpenRead(chromedriverPath))
        {
            foreach (ZipArchiveEntry entry in archive.Entries)
            {
                if (entry.FullName.EndsWith("chromedriver.exe", StringComparison.OrdinalIgnoreCase))
                {
                    destinationPath = Path.Combine(finalpath, entry.FullName);
                    entry.ExtractToFile(destinationPath, overwrite: true);
                    break; // Once extracted, no need to continue looping
                }
            }
        }
        //Log.LogMessage($"destinationPath : {destinationPath}");
        //Log.LogMessage($"Path.Combine(path, \"chromedriver.exe\") : {Path.Combine(path, "chromedriver.exe")}");
        if (File.Exists(Path.Combine(path, "chromedriver.exe")))
        {
            File.Delete(Path.Combine(path, "chromedriver.exe"));
            //Directory.Delete(chromeaddreess);
        }
        System.IO.File.Move(Path.Combine(destinationPath), Path.Combine(path, "chromedriver.exe"));
    }
}
catch (Exception ex)
{
    //Log.LogMessage(ex.Message);
    throw ex;
}