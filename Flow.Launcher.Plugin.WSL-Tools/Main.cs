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

    public async Task InitAsync(PluginInitContext context)
    {
      _context = context;
      _settings = context.API.LoadSettingJsonStorage<Settings>();

      await Task.CompletedTask;
    }

    public Control CreateSettingPanel()
    {
      return new SettingsView(new SettingsViewModel());
    }

    public async Task<List<Result>> QueryAsync(Query query, CancellationToken token)
    {
      Console.WriteLine(query);
      try
      {
        return query.ActionKeyword switch
        {
          "c" => await Task.Run(() => Code.Query(query, _settings, _context)),
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

    public static Settings GetSettings()
    {
      return _settings;
    }

    public static PluginInitContext GetContext()
    {
      return _context;
    }
  }
}
