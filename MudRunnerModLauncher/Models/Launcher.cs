using SharpCompress.Archives.SevenZip;
using SharpCompress.Common;
using SharpCompress.Readers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;

namespace MudRunnerModLauncher.Models
{
	public class Launcher
	{
		private const string MODS_DIR_NAME = "MRMLauncherMods";
		private const string MEDIA = "Media";
		private const string MUD_RUNNER_EXE = "MudRunner.exe";
		private const string CONFIG_XML = "Config.xml";

		private readonly DirectoryInfo _tempDir = new DirectoryInfo($@"{Environment.CurrentDirectory}\Temp");

		private readonly HashSet<string> _modRootFolders =
		[
			"billboards",
			"classes",
			"levels",
			"scripts",
			"sounds",
			"meshcache",
			"texturecache",
			"_m",
			"_t",
			"_templates"
		];

		private readonly DirectoryPathLoader _directoryPathLoader ;


		public Launcher(DirectoryPathLoader? directoryPathLoader = null)
		{
			_directoryPathLoader = directoryPathLoader ?? new DirectoryPathLoader();
			MudRunnerRoorDir = _directoryPathLoader.Load();
		}


		public string MudRunnerRoorDir { get; private set; }

		public string[] AvailableExts { get; } = ["*.zip", "*.rar", "*.7z", "*.tar"];


		public void SaveMudRunnerRoorDir(string mRRootDir)
		{
			_directoryPathLoader.Save(mRRootDir);
			MudRunnerRoorDir = _directoryPathLoader.Load();
		}

		public bool IsCorrectMRRootDir => !string.IsNullOrEmpty(MudRunnerRoorDir) 
											&& File.Exists(@$"{MudRunnerRoorDir.Trim([' ', '\\'])}\{MUD_RUNNER_EXE}")
											&& File.Exists(@$"{MudRunnerRoorDir.Trim([' ', '\\'])}\{CONFIG_XML}");


		public async Task AddModAsync(FileInfo mod)
		{
			await Task.Run(() =>
			{
				if (!mod.Exists || !IsCorrectMRRootDir)
					return;

				var allEntKeys = GetAllEntryKeysWithoutDirs(mod);
				var modDirs = GetOnlyModDirs(allEntKeys);
				if (!modDirs.Any())
					return;
				DirectoryInfo modDectinationDir = new DirectoryInfo($@"{GetModsDir()}\{System.IO.Path.GetFileNameWithoutExtension(mod.Name)}");
				ExtractFiles(mod, modDectinationDir, modDirs);

				AddModPathToConfig(modDectinationDir);

				ModFileCorrection(modDectinationDir);
			});
		}

		public async Task DeleteModAsync(DirectoryInfo mod)
		{
			await Task.Run(() =>
			{
				Directory.Delete(mod.FullName, true);
				DeleteModPathFromConfig(mod);
			});
		}

		public async Task<List<DirectoryInfo>> GetAddedModsAsync()
		{
			return await Task.Run(() =>
			{
				if (!IsCorrectMRRootDir)
					return [];

				DirectoryInfo modsDir = GetModsDir();

				if (!modsDir.Exists)
					return [];

				List<DirectoryInfo> modDirs = Directory.GetDirectories(modsDir.FullName).Select(md => new DirectoryInfo(md)).ToList();

				return modDirs;
			});
		}

		private DirectoryInfo GetModsDir()
		{
			return new DirectoryInfo($@"{MudRunnerRoorDir}\{MEDIA}\{MODS_DIR_NAME}");
		}


		private void AddModPathToConfig(DirectoryInfo modDir)
		{
			ConfigElement addElem = CreateMediaPathElement(modDir);
			List<ConfigElement> allElements = GetAllConfigElements();

			if (allElements.Any(elem => elem == addElem))
				return;

			allElements.Add(addElem);
			FileInfo config = CreateConfig(allElements, _tempDir);

			config.CopyTo($@"{MudRunnerRoorDir}\{config.Name}", true);
			config.Delete();			
		}

		private void DeleteModPathFromConfig(DirectoryInfo modDir)
		{
			ConfigElement delElem = CreateMediaPathElement(modDir);
			IEnumerable<ConfigElement> elements =  GetAllConfigElements().Where(elem => elem != delElem);
			FileInfo config = CreateConfig(elements, _tempDir);

			config.CopyTo($@"{MudRunnerRoorDir}\{config.Name}", true);
			config.Delete();
		}

		private ConfigElement CreateMediaPathElement(DirectoryInfo modDir)
		{
			var elem = new ConfigElement("MediaPath");
			elem.Attributes.Add(new ConfigElementAttribute("Path", @$"{MEDIA}\{MODS_DIR_NAME}\{modDir.Name}"));
			return elem;
		}

		private List<ConfigElement> GetAllConfigElements()
		{
			if (!IsCorrectMRRootDir)
				return [];

			string confPath = $@"{MudRunnerRoorDir}\{CONFIG_XML}";
			List<ConfigElement> elements = [];

			using (Stream stream = File.OpenRead(confPath))
			{
				XmlReaderSettings settings = new XmlReaderSettings();
				settings.Async = true;

				using (XmlReader reader = XmlReader.Create(stream, settings))
				{
					while (reader.Read())
					{
						if (reader.NodeType == XmlNodeType.Element)
						{
							ConfigElement element = new ConfigElement(reader.Name);

							if (reader.AttributeCount > 0)
							{
								while (reader.MoveToNextAttribute())
								{
									element.Attributes.Add(new ConfigElementAttribute(reader.Name, reader.Value));
								}
							}

							elements.Add(element);
						}
					}
				}
			}

			return elements;
		}

		private FileInfo CreateConfig(IEnumerable<ConfigElement> elements, DirectoryInfo destinationDir)
		{
			if(!destinationDir.Exists)
				destinationDir.Create();

			string confPath = @$"{destinationDir.FullName}\{CONFIG_XML}";

			if(File.Exists(confPath))
				File.Delete(confPath);

			using (Stream stream = File.OpenWrite(confPath))
			{
				XmlWriterSettings settings = new XmlWriterSettings();
				settings.Async = true;
				settings.OmitXmlDeclaration = true;
				settings.Indent = true;
				settings.IndentChars = "\t";

				using (XmlWriter writer = XmlWriter.Create(stream, settings))
				{
					ConfigElement firstElem = elements.First();
					writer.WriteStartElement(null, firstElem.Name, null);
					foreach (var attribute in firstElem.Attributes)
					{
						writer.WriteAttributeString(null, attribute.Name, null, attribute.Value);
					}

					foreach (var element in elements.Skip(1))
					{
						settings.NewLineOnAttributes = true;
						writer.WriteStartElement(null, element.Name, null);
						settings.NewLineOnAttributes = false;
						foreach (var attribute in element.Attributes)
						{
							writer.WriteAttributeString(null, attribute.Name, null, attribute.Value);
						}
						writer.WriteEndElement();
					}

					writer.WriteEndElement();
					writer.Flush();
				}
			}

			return new FileInfo(confPath);
		}

		private void ModFileCorrection(DirectoryInfo modRootDir)
		{
			DirectoryInfo levelsDir = new DirectoryInfo(@$"{modRootDir.FullName}\levels");

			if (!levelsDir.Exists)
				return;

			var extensions = new[] { "*.dds", "*.stg" };
			var files = extensions.SelectMany(ext => levelsDir.GetFiles(ext));

			foreach (var file in files)
			{
				if (!file.Name.StartsWith("level_"))
				{
					file.MoveTo($@"{file.Directory}\level_{file.Name}", true);
				}
			}
		}

		private List<string> GetAllEntryKeysWithoutDirs(FileInfo source)
		{
			if(source.Extension == ".7z")
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
						if(!reader.Entry.IsDirectory)
						{
							res.Add(reader.Entry.Key);
						}
					}
				}

				return res;
			}
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
					string modFileDestinatio = string.Empty;
					for (int i = nestingLevel; i < keyInfo.Parts.Length - 1; i++)
					{
						modFileDestinatio += keyInfo.Parts[i] + "\\";
					}

					res.Add(keyInfo.OrigKey, modFileDestinatio.TrimEnd('\\'));
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

		private void ExtractFiles(FileInfo source, DirectoryInfo destination, Dictionary<string, string> actualModDirs)
		{
			foreach(var modDir in actualModDirs.Values.Distinct().Select(md => $@"{destination}\{md}"))
			{
				if(!Directory.Exists(modDir))
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

			if (source.Extension == ".7z")
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
	}

	class KeyInfo
	{
		public KeyInfo(string entryKey)
		{
			OrigKey = entryKey;
			FormatKey = OrigKey.Replace('/', '\\');
			Parts = FormatKey.Split('\\');
			Root = Parts.FirstOrDefault() ?? string.Empty;			
		}

		public string? Root { get; } = string.Empty;
		public string OrigKey { get; } = string.Empty;
		public string FormatKey {  get; } = string.Empty;
		public string[] Parts { get; }
	}

}
