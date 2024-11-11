using Flow.Launcher.Plugin.WSLTools.Core;

namespace Flow.Launcher.Plugin.WSLTools.UI
{
  public class SettingsViewModel : BaseModel
  {
    public bool useSSHGit
    {
      get => WslTools.GetSettings().useSSHGit;
      set
      {
        WslTools.GetSettings().useSSHGit = value;
        OnPropertyChanged();
      }
    }

    public string gitFolder
    {
      get => WslTools.GetSettings().gitFolder;
      set
      {
        WslTools.GetSettings().gitFolder = value;
        OnPropertyChanged();
      }
    }

    public string distroName
    {
      get => WslTools.GetSettings().distroName;
      set
      {
        WslTools.GetSettings().distroName = value;
        OnPropertyChanged();
      }
    }
  }
}
