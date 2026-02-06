using System.Collections.Generic;
using Octokit;

namespace Flow.Launcher.Plugin.WSLTools.Core
{
  public class ApiResult
  {
    public int TotalCount { get; set; }
    public bool IncompleteResults { get; set; }
    public List<Repository> Items { get; set; }
  }
}
