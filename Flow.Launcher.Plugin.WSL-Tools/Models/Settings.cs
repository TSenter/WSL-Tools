namespace Flow.Launcher.Plugin.WSLTools.Core
{
  public class Settings
  {
    public string apiToken { get; set; } = "";
    public string gitFolder { get; set; } = "/git";
    public string distroName { get; set; } = "Ubuntu";
    public bool useSSHGit { get; set; } = false;
  }
}
