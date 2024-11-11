using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Flow.Launcher.Plugin.WSLTools.Core;

namespace Flow.Launcher.Plugin.WSLTools
{
  static class CodeCommand
  {
    private static readonly string icon = "vscode.png";

    public static List<Result> Query(Query query, Settings settings, PluginInitContext context)
    {
      List<Result> results = new List<Result>();

      if (query.Search.Length == 0)
      {
        results.Add(new Result
        {
          Title = "Open VSCode",
          SubTitle = "...or keep typing to search for repositories",
          IcoPath = icon,
          Action = (e) =>
          {
            OpenVSCode("", settings);
            return true;
          }
        });
        return results;
      }

      string searchString = String.Join("*", query.Search.Replace(" ", "").ToCharArray());
      string wslPath = $"\\\\wsl$\\{settings.distroName}\\{settings.gitFolder}";
      string[] directories = Directory.GetDirectories(wslPath, $"*{searchString}*", SearchOption.TopDirectoryOnly);
      string[] workspaces = Directory.GetFiles(wslPath, $"*{searchString}*.code-workspace", SearchOption.TopDirectoryOnly);
      List<string> allPaths = directories.Concat(workspaces).ToList();

      if (allPaths.Exists(results => results.Length > 0))
      {
        foreach (string path in allPaths)
        {
          string fileName = Path.GetFileName(path);
          string title = GetDisplayName(fileName);
          string subTitle = fileName.EndsWith(".code-workspace") ? "Workspace" : "Directory";

          results.Add(new Result
          {
            Title = title,
            SubTitle = subTitle,
            IcoPath = icon,
            Action = (e) =>
            {
              OpenVSCode(path, settings);
              return true;
            }
          });
        }
      }
      else
      {
        results.Add(new Result
        {
          Title = "No results found",
          SubTitle = "Try searching for something else",
          IcoPath = icon
        });
      }

      return results;
    }

    private static void OpenVSCode(string path, Settings settings)
    {
      string fileName = Path.GetFileName(path);
      string wslNetworkPath = $"\\\\wsl$\\{settings.distroName}";
      string gitFolder = settings.gitFolder.Replace("/", "\\");

      string command = $"wsl --cd {wslNetworkPath}\\{gitFolder} --distribution {settings.distroName} code {fileName}";

      ProcessStartInfo info = new ProcessStartInfo
      {
        FileName = "cmd.exe",
        Arguments = $"/c {command}",
        UseShellExecute = true,
        WindowStyle = ProcessWindowStyle.Hidden
      };

      Process.Start(info);
    }

    private static string GetDisplayName(string fileName)
    {
      if (!fileName.EndsWith(".code-workspace"))
      {
        return fileName;
      }
      return fileName.Replace(".code-workspace", "");
    }
  }
}
