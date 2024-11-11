using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Flow.Launcher.Plugin.WSLTools.Core;
using Octokit;

namespace Flow.Launcher.Plugin.WSLTools
{
  static class UpdateCommand
  {
    private static readonly string icon = "github.png";
    private static readonly string owner = "TSenter";
    private static readonly string repoName = "WSL-Tools";
    private static readonly Regex assetNamePattern = new Regex(@"WSL-Tools-v(?:.+).zip");

    public static async Task<List<Result>> Query(Settings settings, PluginInitContext context)
    {
      List<Result> list = new List<Result>();

      if (string.IsNullOrEmpty(settings.apiToken))
      {
        list.Add(new Result
        {
          Title = $"Current Version: {WslTools.GetVersion()}",
          SubTitle = "GitHub API token is required to check for updates",
          IcoPath = icon
        });
        return list;
      }

      IReadOnlyList<Release> releaseResults = await GithubApi.Client.Repository.Release.GetAll(owner, repoName);

      IOrderedEnumerable<Release> orderedResults = releaseResults.OrderByDescending(r => r.PublishedAt);
      List<DownloadableAsset> assets = GetDownloadableAssets(orderedResults);

      if (assets.Count == 0)
      {
        list.Add(new Result
        {
          Title = "No Results Found",
          IcoPath = icon
        });
        return list;
      }

      foreach (DownloadableAsset asset in assets)
      {
        Result result = new Result
        {
          IcoPath = icon
        };
        if (asset.IsLatestRelease)
        {
          result.SubTitle = $"Latest Release";
        }
        else if (asset.IsPreRelease)
        {
          result.SubTitle = $"Pre-Release";
        }

        if (asset.IsCurrent)
        {
          result.Title = $"Current Version: {asset.Version}";
          result.IcoPath = icon;
        }
        else
        {
          result.Title = $"{asset.UpdateAction} to {asset.Version}";
          result.Action = (e) =>
          {
            UpdatePlugin(asset.Url, context);
            return true;
          };

        }
        list.Add(result);
      }

      return list;
    }

    private static List<DownloadableAsset> GetDownloadableAssets(IEnumerable<Release> releases)
    {
      Version currentVersion = WslTools.GetVersion();
      List<DownloadableAsset> assets = new List<DownloadableAsset>();

      bool foundLatestRelease = false;
      foreach (Release release in releases)
      {
        foreach (ReleaseAsset asset in release.Assets)
        {
          bool isLatestRelease = false;
          if (!foundLatestRelease && !release.Prerelease)
          {
            isLatestRelease = true;
            foundLatestRelease = true;
          }
          string updateAction = "Downgrade";
          Version releaseVersion = new Version(release.TagName);
          if (releaseVersion > currentVersion)
          {
            updateAction = "Upgrade";
          }
          else if (releaseVersion == currentVersion)
          {
            updateAction = "Current";
          }

          if (assetNamePattern.IsMatch(asset.Name))
          {
            assets.Add(new DownloadableAsset
            {
              Version = release.Name,
              IsLatestRelease = isLatestRelease,
              IsPreRelease = release.Prerelease,
              UpdateAction = updateAction,
              IsCurrent = releaseVersion == currentVersion,
              Url = asset.Url,
            });
          }
        }
      }
      return assets;
    }

    private static void UpdatePlugin(string releaseAssetUrl, PluginInitContext context)
    {
      string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
      string pluginsDirectory = $"{appData}\\FlowLauncher\\Plugins";
      string destinationZipFile = $"{pluginsDirectory}\\Flow.Launcher.Plugin.WSL-Tools.zip";
      string destinationPluginDirectory = $"{pluginsDirectory}\\Flow.Launcher.Plugin.WSL-Tools";

      if (!Directory.Exists(pluginsDirectory))
      {
        context.API.ShowMsg("WSL-Tools update failed", "Flow Launcher plugins directory not found", icon);
        return;
      }

      if (File.Exists(destinationZipFile))
      {
        File.Delete(destinationZipFile);
      }

      GithubApi.DownloadFile(releaseAssetUrl, destinationZipFile, "application/octet-stream");

      string shellArguments = $"Stop-Process -Name Flow.Launcher -Force;";
      shellArguments += $"Expand-Archive -Force -LiteralPath '{destinationZipFile}' -DestinationPath {destinationPluginDirectory};";
      shellArguments += $"Start-Process -FilePath \"$env:LOCALAPPDATA\\FlowLauncher\\Flow.Launcher.exe\";";
      shellArguments += $"Remove-Item -Path '{destinationZipFile}' -Force;";

      ProcessStartInfo processInfo = new ProcessStartInfo
      {
        LoadUserProfile = true,
        FileName = "powershell.exe",
        Arguments = shellArguments,
        RedirectStandardOutput = false,
        UseShellExecute = true,
        CreateNoWindow = true,
        WindowStyle = ProcessWindowStyle.Hidden
      };

      Process.Start(processInfo);
    }
  }

  class DownloadableAsset
  {
    public string Version { get; set; }
    public bool IsLatestRelease { get; set; }
    public bool IsPreRelease { get; set; }
    public string UpdateAction { get; set; }
    public bool IsCurrent { get; set; }
    public string Url { get; set; }
  }
}
