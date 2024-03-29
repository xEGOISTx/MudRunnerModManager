using SharpCompress.Archives.SevenZip;
using SharpCompress.Common;
using SharpCompress.Readers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MudRunnerModManager.Models.ArchiveWorker
{
	internal class ArchiveExtractor
	{
		public static string[] AvailableExts { get; } = ["*.zip", "*.rar", $"*{AppConsts.SEVEN_ZIP_EXT}", "*.tar"];

		public List<string> GetAllEntryKeysWithoutDirs(FileInfo source)
		{
			if (source.Extension == AppConsts.SEVEN_ZIP_EXT)
			{
				using (var sevenZA = SevenZipArchive.Open(source))
				{
					return sevenZA.Entries.Where(ent => !ent.IsDirectory).Select(entry => entry.Key).ToList();
				}
			}
			else
			{
				List<string> res = [];
				using (Stream stream = File.OpenRead(source.FullName))
				using (var reader = ReaderFactory.Open(stream))
				{
					while (reader.MoveToNextEntry())
					{
						if (!reader.Entry.IsDirectory)
						{
							res.Add(reader.Entry.Key);
						}
					}
				}

				return res;
			}
		}

		public async Task<List<string>> GetAllEntryKeysWithoutDirsAsync(FileInfo source)
		{
			return await Task.Run(() =>
			{
				return GetAllEntryKeysWithoutDirs(source);
			});
		}

		/// <summary>
		/// Run extract files
		/// </summary>
		/// <param name="source">Arhive file</param>
		/// <param name="destination"></param>
		/// <param name="actualModDirs">Key: Entry key, Value: actual path without file name</param>
		public void Extract(FileInfo source, DirectoryInfo destination, Dictionary<string, string> actualModDirs)
		{
			foreach (var modDir in actualModDirs.Values.Distinct().Select(md => $@"{destination}\{md}"))
			{
				if (!Directory.Exists(modDir))
					Directory.CreateDirectory(modDir);
			}

			void Ext(IReader reader)
			{
				while (reader.MoveToNextEntry())
				{
					if (!reader.Entry.IsDirectory)
					{
						if (actualModDirs.TryGetValue(reader.Entry.Key, out string? filePath))
						{
							string destDir = @$"{destination.FullName}\{filePath}";

							reader.WriteEntryToDirectory(destDir, new ExtractionOptions()
							{
								Overwrite = true
							});
						}
					}
				}
			}

			if (source.Extension == AppConsts.SEVEN_ZIP_EXT)
			{
				using (var reader = SevenZipArchive.Open(source).ExtractAllEntries())
				{
					Ext(reader);
				}
			}
			else
			{
				using (Stream stream = File.OpenRead(source.FullName))
				{
					using (var reader = ReaderFactory.Open(stream))
					{
						Ext(reader);
					}
				}
			}
		}

		/// <summary>
		/// Run extract files async
		/// </summary>
		/// <param name="source">Arhive file</param>
		/// <param name="destination"></param>
		/// <param name="actualModDirs">Key: Entry key, Value: actual path without file name</param>
		public async Task ExtractAsync(FileInfo source, DirectoryInfo destination, Dictionary<string, string> actualModDirs)
		{
			await Task.Run(() =>
			{
				Extract(source, destination, actualModDirs);
			});
		}
	}

}
