using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace MudRunnerModManager.Common
{
	public class GitHubRepo
	{
		private readonly static Dictionary<int, string> _userAgents = new()
		{ 
			[0] = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/87.0.4280.88 Safari/537.36",
			[1] = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/109.0.0.0 Safari/537.36",
			[2] = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/89.0.4389.105 YaBrowser/21.3.3.230 Yowser/2.5 Safari/537.36",
			[3] = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/92.0.4515.131 YaBrowser/21.8.1.468 Yowser/2.5 Safari/537.36",
			[4] = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/87.0.4280.88 YaBrowser/20.12.2.105 Yowser/2.5 Safari/537.36",
			[5] = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/89.0.4389.90 Safari/537.36",
			[6] = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/92.0.4515.131 Safari/537.36",
			[7] = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:85.0) Gecko/20100101 Firefox/85.0",
			[8] = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/85.0.4183.102 YaBrowser/20.9.3.136 Yowser/2.5 Safari/537.36",
			[9] = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:89.0) Gecko/20100101 Firefox/89.0",
		};


		public static async Task<AppVersion?> GetLatestVersionAsync()
		{
			try 
			{
				JsonNode? versionInfo = await GetLatestVersionInfo();
				return versionInfo != null ? await ToAppVersion(versionInfo) : null;
			}
			catch (Exception ex)
			{
				//пока просто заглушка т.к. не важно
				return null;
			}
		}

		private static async Task<JsonNode?> GetLatestVersionInfo()
		{
			Random rand = new();
			var r = rand.Next(0, 9);

			using var client = new HttpClient();

			using HttpRequestMessage request = new(HttpMethod.Get, "https://api.github.com/repos/xEGOISTx/MudRunnerModManager/releases/latest");
			request.Headers.Add("user-agent", _userAgents[r]);
			request.Headers.Add("Accept", "application/vnd.github+json");
			request.Headers.Add("X-GitHub-Api-Version", "2022-11-28");

			var response = await client.SendAsync(request);
			if (response == null || response.StatusCode != System.Net.HttpStatusCode.OK)
				return null;

			string res = await response.Content.ReadAsStringAsync();
			return JsonNode.Parse(res);
		}

		public static void OpenRepo()
		{
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			{
				Process.Start(new ProcessStartInfo("cmd", $"/c start https://github.com/xEGOISTx/MudRunnerModManager"));
			}
		}

		public static void OpenReleases()
		{
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			{
				Process.Start(new ProcessStartInfo("cmd", $"/c start https://github.com/xEGOISTx/MudRunnerModManager/releases"));
			}
		}

		private static async Task<AppVersion?> ToAppVersion(JsonNode versionInfo)
		{
			AppVersion? appVersion = null;
			await Task.Run(() =>
			{
				string? version = (string?)versionInfo["tag_name"];

				if (version != null)
				{
					string[] ver = version.Split('.');
					if (ver.Length == 3)
					{
						if (int.TryParse(ver[0], out int major)
						&& int.TryParse(ver[1], out int minor)
						&& int.TryParse(ver[2], out int build))
						{
							appVersion = new AppVersion(major, minor, build);
						}					
					}
				}
			});

			return appVersion;
		}
	}
}
