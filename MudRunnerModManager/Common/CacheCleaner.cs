using System.IO;

namespace MudRunnerModManager.Common
{
	public class CacheCleaner
	{
		private readonly DirectoryInfo _cacheDirectory;

		public CacheCleaner(DirectoryInfo cacheDirectory)
		{
			_cacheDirectory = cacheDirectory;
		}

		public bool IsPresentCache()
		{
			if (Directory.Exists(_cacheDirectory.FullName))
			{
				if (_cacheDirectory.GetDirectories().Length > 0 || _cacheDirectory.GetFiles().Length > 0)
					return true;
			}

			return false;
		}

		public void ClearCache()
		{
			if (Directory.Exists(_cacheDirectory.FullName))
			{
				foreach (var dir in _cacheDirectory.GetDirectories())
				{
					dir.Delete(true);
				}

				foreach (var file in _cacheDirectory.GetFiles())
				{
					file.Delete();
				}
			}
		}
	}
}
