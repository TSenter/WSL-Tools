using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Octokit;

namespace Flow.Launcher.Plugin.WSLTools.Core
{
  class GithubApi
  {
    private static readonly HttpClient httpClient = new HttpClient();
    private static GithubApi instance = null;
    private GitHubClient client;

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
  }
}
