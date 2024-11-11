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
    private static PluginInitContext _context;
    private static Settings _settings;
    private static Version version;

    public async Task InitAsync(PluginInitContext context)
    {
      _context = context;
      _settings = context.API.LoadSettingJsonStorage<Settings>();

      if (!string.IsNullOrEmpty(_settings.apiToken))
      {
        GithubApi.Init(context, _settings);
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
          "c" => await Task.Run(() => CodeCommand.Query(query, _settings, _context)),
          "wt" => await Task.Run(() => UpdateCommand.Query(_settings, _context)),
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
      if (string.IsNullOrEmpty(_settings.apiToken))
      {
        return;
      }

      string priorVersion = _settings.version;

      if (priorVersion == null) {
        _settings.version = version.ToString();
        _context.API.SaveSettingJsonStorage<Settings>();

        _context.API.ShowMsg("WSL-Tools has been installed");

        return;
      }

      if (version != null && priorVersion != version.ToString())
      {
        _settings.version = version.ToString();
        _context.API.SaveSettingJsonStorage<Settings>();

        _context.API.ShowMsg("WSL-Tools has been updated to version " + version);
      }
    }

    public static Settings GetSettings()
    {
      return _settings;
    }

    public static PluginInitContext GetContext()
    {
      return _context;
    }

    public static Version GetVersion()
    {
      return version;
    }
  }
}
