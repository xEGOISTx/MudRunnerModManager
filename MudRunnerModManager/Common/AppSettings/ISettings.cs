using System.Collections.Generic;
using System.IO;

namespace MudRunnerModManager.Common.AppSettings
{
	public interface ISettings
	{
		string MudRunnerRootDir { get; set; }
		bool AlwaysClearCache {  get; set; }
		bool DeleteModWithoutWarning { get; set; }
		List<DirectoryInfo> Chapters { get; set; }
	}
}
