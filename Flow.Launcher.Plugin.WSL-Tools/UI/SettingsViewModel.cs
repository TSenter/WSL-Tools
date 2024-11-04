using Flow.Launcher.Plugin.WSLTools.Core;

namespace Flow.Launcher.Plugin.WSLTools.UI
{
  public class SettingsViewModel : BaseModel
  {
    public readonly PluginInitContext Context;
    public Settings Settings { get; init; }

    public SettingsViewModel(PluginInitContext context, Settings settings)
    {
      Settings = settings;
      Context = context;
    }

    public bool useSSHGit
    {
      get => Settings.useSSHGit;
      set
      {
        Settings.useSSHGit = value;
        OnPropertyChanged();
      }
    }

    public string gitFolder
    {
      get => Settings.gitFolder;
      set
      {
        Settings.gitFolder = value;
        OnPropertyChanged();
      }
    }

    public string distroName
    {
      get => Settings.distroName;
      set
      {
        Settings.distroName = value;
        OnPropertyChanged();
      }
    }
  }
}
