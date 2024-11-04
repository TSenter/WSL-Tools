using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using Flow.Launcher.Plugin;
using Flow.Launcher.Plugin.WSLTools.Core;
using Flow.Launcher.Plugin.WSLTools.UI;

namespace Flow.Launcher.Plugin.WSLTools
{
  public class WslTools : IAsyncPlugin, ISettingProvider
  {
    private PluginInitContext context;
    private Settings settings;

    public async Task InitAsync(PluginInitContext context)
    {
      this.context = context;
      settings = context.API.LoadSettingJsonStorage<Settings>();

      await Task.CompletedTask;
    }

    public Control CreateSettingPanel()
    {
      return new SettingsView(new SettingsViewModel(context, settings));
    }

    public async Task<List<Result>> QueryAsync(Query query, CancellationToken token)
    {
      Console.WriteLine(query);
      try
      {
        return query.ActionKeyword switch
        {
          "c" => await Task.Run(() => Code.Query(query, settings, context)),
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
  }
}
