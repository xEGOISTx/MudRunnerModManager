using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace MudRunnerModManager.Common.AppSettings
{
    public abstract class SettingsBase
    {
        public SettingsBase()
        {
            Chapters = new ReadOnlyCollection<DirectoryInfo>(ChaptersI);
        }

        public string MudRunnerRootDir { get; protected set; } = string.Empty;

		public bool AlwaysClearCache { get; protected set; }

		public bool DeleteModWithoutWarning { get; protected set; }

		public ReadOnlyCollection<DirectoryInfo> Chapters { get; }

        protected List<DirectoryInfo> ChaptersI { get; } = [];
    }
}
