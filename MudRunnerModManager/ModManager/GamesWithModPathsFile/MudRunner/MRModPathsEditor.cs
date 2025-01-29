using MudRunnerModManager.Common;

namespace MudRunnerModManager.ModManager.GamesWithModPathsFile.MudRunner
{
	public class MRModPathsEditor : ModPathsEditorXmlBase
	{
		public MRModPathsEditor(string mudRunnerRootPath) : 
			base($@"{mudRunnerRootPath}\{XmlConsts.CONFIG_XML}", 
				$@"{AppConsts.MEDIA}\{AppConsts.MR_MODS_ROOT_DIR_NAME}", 
				AppConsts.MR_MODS_ROOT_DIR_NAME, XmlConsts.CONFIG)
		{ }
	}
}
