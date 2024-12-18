using System;
using System.IO;

namespace MudRunnerModManager.Common.Exstensions
{
	public static class DirInfoExtension
	{
		public static long GetSize(this DirectoryInfo dirInfo)
		{
			long size = 0;

			FileInfo[] files = dirInfo.GetFiles();
			foreach (FileInfo file in files)
			{
				size += file.Length;
			}

			DirectoryInfo[] dirs = dirInfo.GetDirectories();
			foreach (DirectoryInfo dir in dirs)
			{
				size += dir.GetSize();
			}
			return size;
		}
	}
}
