using System.IO;

namespace MudRunnerModManager.Common
{
	public class CacheCleaner
	{
		public CacheCleaner(DirectoryInfo cacheDirectory)
		{
			CacheDirectory = cacheDirectory;
		}

		public DirectoryInfo CacheDirectory { get; }

		public bool IsPresentCache()
		{
			if (Directory.Exists(CacheDirectory.FullName))
			{
				if (CacheDirectory.GetDirectories().Length > 0 || CacheDirectory.GetFiles().Length > 0)
					return true;
			}

			return false;
		}

		public void ClearCache()
		{
			if (Directory.Exists(CacheDirectory.FullName))
			{
				foreach (var dir in CacheDirectory.GetDirectories())
				{
					dir.Delete(true);
				}

				foreach (var file in CacheDirectory.GetFiles())
				{
					file.Delete();
				}
			}
		}
	}
}
