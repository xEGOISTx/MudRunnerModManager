using MudRunnerModManager.Common;

namespace MudRunnerModManager.ModManager.GamesWithModPathsFile.SpinTires
{
	public class STModPathsEditor : ModPathsEditorXmlBase
	{
		public STModPathsEditor(string spinTiresRootPath) :
			base($@"{spinTiresRootPath}\{AppConsts.USER}\{XmlConsts.USER_MEDIA_PATHS_XML}",
			$@"{AppConsts.MEDIA}\{AppConsts.ST_MODS_ROOT_DIR_NAME}",
			AppConsts.ST_MODS_ROOT_DIR_NAME, XmlConsts.DATA_SOURCES)
		{

		}
	}
}
