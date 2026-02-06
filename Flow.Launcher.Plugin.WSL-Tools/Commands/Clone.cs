using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Flow.Launcher.Plugin.WSLTools.Core;
using Octokit;

namespace Flow.Launcher.Plugin.WSLTools
{
  static class CloneCommand
  {
    private static readonly string ico = "github.png";

    public static void clone(Repository result)
    {
      string cloneUrl = WslTools.GetSettings().useSSHGit ? result.SshUrl : result.CloneUrl;
      clone(cloneUrl, result.Name);
    }

    public static void clone(string cloneUrl, string repoName)
    {
      Settings settings = WslTools.GetSettings();
      string distro = settings.distroName;
      string cloneDirectory = $"\\\\wsl$\\{distro}{settings.gitFolder}";

      string targetDirectory = $"{cloneDirectory}\\{repoName}";
      if (Directory.Exists(targetDirectory))
      {
        return;
      }

      string command = $"wsl --cd {settings.gitFolder} --distribution {distro} git clone {cloneUrl}";

      string arguments = $"/c \"{command}\"";

      ProcessStartInfo info;
      info = new ProcessStartInfo
      {
        FileName = "cmd.exe",
        Arguments = arguments,
        UseShellExecute = true,
        WindowStyle = ProcessWindowStyle.Hidden,
        WorkingDirectory = settings.gitFolder
      };

      Process.Start(info);
    }

    public static async Task<List<Result>> Query(Query query)
    {
      List<Result> list = new List<Result>();

      string searchQuery = query.Search;
      string cloneMessage = "Clone repository";

      if (searchQuery.Length == 0)
      {
        list.Add(new Result
        {
          Title = cloneMessage,
          SubTitle = "Keep typing to search for repositories",
          IcoPath = ico
        });
        return list;
      }

      Settings settings = WslTools.GetSettings();
      string folder = $"\\\\wsl$\\{settings.distroName}{settings.gitFolder}";

      if (searchQuery.StartsWith("https://") || searchQuery.StartsWith("git@"))
      {
        int lastSlash = searchQuery.LastIndexOf('/');
        int gitExtension = searchQuery.LastIndexOf(".git");
        string repoName = searchQuery.Substring(lastSlash + 1, gitExtension - lastSlash - 1);

        list.Add(new Result
        {
          Title = repoName,
          SubTitle = cloneMessage,
          IcoPath = ico,
          Action = (e) =>
          {
            // clone(searchQuery, repoName, settings);
            // VSCode.openVSCode(folder + "\\" + repoName, settings);
            return true;
          }
        });
        return list;
      }

      ApiResult results = await GithubApi.Query(searchQuery);

      if (results.TotalCount > 0)
      {
        foreach (Repository result in results.Items)
        {
          list.Add(new Result
          {
            Title = result.FullName,
            SubTitle = cloneMessage,
            IcoPath = ico,
            Action = (e) =>
            {
              // clone(result, settings);
              CodeCommand.OpenVSCode(folder + "\\" + result.Name, settings);
              return true;
            }
          });
        }
      }
      else
      {
        list.Add(new Result
        {
          Title = "No Results Found",
          IcoPath = ico
        });
      }

      return list;
    }
  }
}
