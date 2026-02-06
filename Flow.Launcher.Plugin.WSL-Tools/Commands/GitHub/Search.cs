using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Flow.Launcher.Plugin.WSLTools.Core;
using Octokit;

namespace Flow.Launcher.Plugin.WSLTools
{
  class SearchCommand
  {
    protected static string icon = "github.png";
    protected static int showCount = 10;

    public static async Task<List<Result>> Query(Query query)
    {
      List<Result> list = new List<Result>();

      string[] searchQuery = query.Search.Split(" ");
      string message = "Search GitHub";


      if (string.IsNullOrWhiteSpace(query.Search))
      {
        list.Add(new Result
        {
          Title = message,
          SubTitle = $"{query.ActionKeyword} <owner> <repo>",
          IcoPath = icon
        });

        return list;
      }

      WslTools.GetContext().API.ShowMsg($"Searching GitHub... '{query.Search}' ({searchQuery.Length})");

      if (searchQuery.Length == 1)
      {
        return await QueryOwners(query, searchQuery[0]);
      }

      // ApiResult results = await GithubApi.Query(searchQuery);

      // if (results.TotalCount > 0)
      // {
      //   foreach (Repository result in results.Items)
      //   {
      //     list.Add(new Result
      //     {
      //       Title = result.FullName,
      //       SubTitle = message,
      //       IcoPath = icon,
      //       Action = (e) =>
      //       {
      //         Action(result);
      //         return true;
      //       }
      //     });
      //   }
      // }
      // else
      // {
      //   list.Add(new Result
      //   {
      //     Title = "No results found",
      //     IcoPath = icon
      //   });
      // }

      return list;
    }

    public static Result Map(Repository repo)
    {
      return new Result
      {
        Title = repo.FullName,
        SubTitle = "Open on GitHub",
        IcoPath = icon,
        CopyText = repo.Url,
        Action = (e) => Action(e, repo),
      };
    }

    private static async Task<List<Result>> QueryOwners(Query query, string search)
    {
      SearchUsersResult result = await GithubApi.Client.Search.SearchUsers(new SearchUsersRequest(search));
      List<Result> results = new List<Result>(
        result.Items.Take(showCount).Select((user) =>
        {
          string subTitle = user.Type == AccountType.User ? "User" : "Organization";

          if (result.Items.Count == 1) {
            subTitle = "Keep typing to search repositories";
          }

          return new Result
          {
            Title = user.Login,
            SubTitle = subTitle,
            IcoPath = icon,
            Action = (e) =>
            {
              WslTools.GetContext().API.ChangeQuery($"{query.ActionKeyword} {user.Login} ");
              return false;
            }
          };
        }
      ));

      if (result.TotalCount > showCount)
      {
        results.Add(new Result
        {
          Title = $"{result.TotalCount - showCount} more results...",
          SubTitle = "Keep typing to search for accounts",
          IcoPath = icon,
          Action = (e) => false
        });
      }

      return results;
    }

    private static async Task<IReadOnlyList<Repository>> QueryRepositoriesByOwner(string owner)
    {
      User user = await GithubApi.Client.User.Get(owner);
      if (user != null)
      {
        return await GithubApi.Client.Repository.GetAllForUser(owner);
      }

      Organization org = await GithubApi.Client.Organization.Get(owner);
      if (org != null)
      {
        return await GithubApi.Client.Repository.GetAllForOrg(owner);
      }

      return null;
    }

    public static bool Action(ActionContext context, Repository repo)
    {
      WslTools.GetContext().API.OpenUrl(repo.Url);
      return true;
    }
  }
}
