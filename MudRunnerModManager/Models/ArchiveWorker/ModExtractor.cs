using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MudRunnerModManager.Common;

namespace MudRunnerModManager.Models.ArchiveWorker
{
    internal class ModExtractor(ArchiveExtractor aExtractor)
	{
		private readonly ArchiveExtractor _aExtractor = aExtractor;
		private readonly HashSet<string> _modRootFolders =
		[
			"billboards",
			"classes",
			"joysticks",
			"levels",
			"scripts",
			"sounds",
			"meshcache",
			"texturecache",
			"_m",
			"_t",
			"_templates"
		];

		public async Task<bool> ExtractAsync(FileInfo mod, DirectoryInfo destination)
		{
			return await Task.Run(async () =>
			{
				var allEntryKeys = await _aExtractor.GetAllEntryKeysWithoutDirsAsync(mod);
				var modDirs = GetOnlyModDirs(allEntryKeys);

				if (!modDirs.Any())
					return false;

				await _aExtractor.ExtractAsync(mod, destination, modDirs);
				return true;
			});

		}

		private Dictionary<string, string> GetOnlyModDirs(List<string> allEntryKeysWithoutDirs)
		{
			Dictionary<string, string> res = [];

			IEnumerable<KeyInfo> keyInfos = allEntryKeysWithoutDirs
				.Select(entryKey => new KeyInfo(entryKey))
				.Where(keyI => !string.IsNullOrEmpty(keyI.Root));

			int nestingLevel = 0;

			void FillModDirs()
			{
				foreach (var keyInfo in keyInfos)
				{
					string modFileDestination = string.Empty;
					for (int i = nestingLevel; i < keyInfo.Parts.Length - 1; i++)
					{
						modFileDestination += keyInfo.Parts[i] + "\\";
					}

					res.Add(keyInfo.OrigKey, modFileDestination.TrimEnd('\\'));
				}
			}

			if (keyInfos.Any(keyI => _modRootFolders.Contains(keyI.Root.ToLower())))
			{
				FillModDirs();
			}
			else
			{
				var keysGroupByRootDir = keyInfos
					.GroupBy(keyI => keyI.Root);

				var media = keysGroupByRootDir.Where(gr => gr.Key == AppConsts.MEDIA);
				if (media.Any())
				{
					var temp = new List<IGrouping<string, KeyInfo>>(media);
					temp.AddRange(keysGroupByRootDir.Where(gr => gr.Key != AppConsts.MEDIA));
					keysGroupByRootDir = temp;

				}

				bool isFind = false;

				foreach (var group in keysGroupByRootDir)
				{
					foreach (var keyInfo in group)
					{
						for (int i = 0; i < keyInfo.Parts.Length; i++)
						{
							if (_modRootFolders.Contains(keyInfo.Parts[i].ToLower()))
							{
								keyInfos = group.ToList();
								nestingLevel = i;
								isFind = true;
								break;
							}
						}

						if (isFind)
							break;
					}

					if (isFind)
						break;
				}

				if (isFind)
					FillModDirs();
			}

			return res;
		}
	}

	class KeyInfo
	{
		public KeyInfo(string entryKey)
		{
			OrigKey = entryKey;
			FormatKey = OrigKey.Replace('/', '\\');
			Parts = FormatKey.Split('\\');
			Root = Parts.First() ?? string.Empty;
		}

		public string Root { get; } = string.Empty;
		public string OrigKey { get; } = string.Empty;
		public string FormatKey { get; } = string.Empty;
		public string[] Parts { get; }
	}
}
