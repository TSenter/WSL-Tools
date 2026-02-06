using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Octokit;

namespace Flow.Launcher.Plugin.WSLTools.Core
{
  class GithubApi
  {
    private static readonly Dictionary<string, Repository> repoCache = new Dictionary<string, Repository>();
    private static readonly HttpClient httpClient = new HttpClient();
    private static GithubApi instance = null;
    private GitHubClient client;

    private static void AddToLocalCache(IEnumerable<Repository> repos)
    {
      foreach (Repository repo in repos)
      {
        if (!repoCache.ContainsKey(repo.FullName))
        {
          repoCache.Add(repo.FullName, repo);
        }
      }
    }

    public static async Task LoadReposToCache()
    {
      if (string.IsNullOrEmpty(WslTools.GetSettings().apiToken))
      {
        return;
      }
      IReadOnlyList<Repository> repos = await instance.client.Repository.GetAllForCurrent();
      AddToLocalCache(repos.ToList());
    }

    public static void Init(PluginInitContext context, Settings settings)
    {
      instance = new GithubApi();
      if (string.IsNullOrEmpty(settings.apiToken))
      {
        return;
      }
      Credentials credentials = new Credentials(settings.apiToken);
      Octokit.ProductHeaderValue productInformation = new Octokit.ProductHeaderValue("Flow.Launcher.Plugin.WSL-Tools", context.CurrentPluginMetadata.Version.ToString());
      instance.client = new GitHubClient(productInformation);
      instance.client.Credentials = credentials;
    }

    public static GitHubClient Client
    {
      get
      {
        return instance.client;
      }
    }

    public static void DownloadFile(string url, string destination, string accept = null)
    {
      ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
      HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);

      request.Headers.UserAgent.TryParseAdd(((Connection)instance.client.Connection).UserAgent);
      request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", instance.client.Credentials.GetToken());

      if (accept != null)
      {
        request.Headers.Accept.TryParseAdd(accept);
      }
      HttpResponseMessage response = httpClient.Send(request);
      using Stream stream = response.Content.ReadAsStream();
      using FileStream fileStream = new FileStream(destination, System.IO.FileMode.OpenOrCreate);
      stream.CopyTo(fileStream);
    }

    private static ApiResult QueryLocalCache(string query)
    {
      char[] splitChars = query.Replace(" ", "").ToCharArray();
      List<string> splitQuery = new List<string>();

      for (int i = 0; i < splitChars.Length; i++)
      {
        splitQuery.Add(Regex.Escape(splitChars[i].ToString()));
      }

      string regexQueryString = string.Join(".*", splitQuery);
      Regex searchRegex = new Regex(".*" + regexQueryString + ".*", RegexOptions.IgnoreCase);

      ApiResult response = new ApiResult();

      response.Items = repoCache
        .Where(i => searchRegex.IsMatch(i.Value.Name) || searchRegex.IsMatch(i.Value.FullName))
        .Select(i => i.Value)
        .OrderBy(i => i.Archived)
          .ThenByDescending(i => i.Owner.Type == AccountType.Organization)
          .ThenByDescending(i => i.UpdatedAt)
        .ToList();

      response.TotalCount = response.Items.Count;
      response.IncompleteResults = response.TotalCount == 0;

      return response;
    }

    public static async Task<ApiResult> Query(string query, SearchRepositoriesRequest request = null)
    {
      ApiResult localResult = QueryLocalCache(query);

      if (!localResult.IncompleteResults)
      {
        return localResult;
      }

      SearchRepositoryResult queryResult = await QueryGithubDotCom(query);
      ApiResult result = new ApiResult();

      result.Items.AddRange(queryResult.Items);

      return result;
    }

    public static async Task<SearchRepositoryResult> QueryGithubDotCom(string query)
    {
      SearchRepositoriesRequest request = new SearchRepositoriesRequest(query);
      request.SortField = RepoSearchSort.Updated;
      request.User = "org:knoxville-utilities-board";

      SearchRepositoryResult result = await instance.client.Search.SearchRepo(request);

      AddToLocalCache(result.Items.ToList());

      return result;
    }
  }
}
