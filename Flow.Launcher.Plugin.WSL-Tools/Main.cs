using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using Flow.Launcher.Plugin.WSLTools.Core;
using Flow.Launcher.Plugin.WSLTools.UI;

namespace Flow.Launcher.Plugin.WSLTools
{
  public class WslTools : IAsyncPlugin, ISettingProvider
  {
    private static PluginInitContext context;
    private static Settings settings;
    private static Version version;

    public async Task InitAsync(PluginInitContext context)
    {
      WslTools.context = context;
      settings = context.API.LoadSettingJsonStorage<Settings>();

      if (!string.IsNullOrEmpty(settings.apiToken))
      {
        GithubApi.Init(context, settings);
        await GithubApi.LoadReposToCache();
      }

      try
      {
        version = new Version(context.CurrentPluginMetadata.Version);
      }
      catch (Exception)
      {
        version = null;
      }

      CheckIfUpdated();

      await Task.CompletedTask;
    }

    public Control CreateSettingPanel()
    {
      return new SettingsView(new SettingsViewModel());
    }

    public async Task<List<Result>> QueryAsync(Query query, CancellationToken token)
    {
      try
      {
        return query.ActionKeyword switch
        {
          "c" => await Task.Run(() => CodeCommand.Query(query, settings, context)),
          "wt" => await Task.Run(() => UpdateCommand.Query(settings, context)),
          "clone" => await Task.Run(() => SearchCommand.Query(query)),
          _ => await Task.Run(() => new List<Result> {
            new Result {
              Title = "Unknown action keyword - '" + query.ActionKeyword + "'",
            }
          }),
        };
      }
      catch (Exception e)
      {
        return new List<Result> {
          new Result {
            Title = "Error",
            SubTitle = e.Message
               }
        };
      }
    }

    private static void CheckIfUpdated() {
      if (string.IsNullOrEmpty(settings.apiToken))
      {
        return;
      }

      string priorVersion = settings.version;

      if (priorVersion == null) {
        settings.version = version.ToString();
        context.API.SaveSettingJsonStorage<Settings>();

        context.API.ShowMsg("WSL-Tools has been installed");

        return;
      }

      if (version != null && priorVersion != version.ToString())
      {
        settings.version = version.ToString();
        context.API.SaveSettingJsonStorage<Settings>();

        context.API.ShowMsg("WSL-Tools has been updated to version " + version);
      }
    }

    public static Settings GetSettings()
    {
      return settings;
    }

    public static PluginInitContext GetContext()
    {
      return context;
    }

    public static Version GetVersion()
    {
      return version;
    }
  }
}
