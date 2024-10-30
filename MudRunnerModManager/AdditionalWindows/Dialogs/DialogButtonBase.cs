using System;

namespace MudRunnerModManager.AdditionalWindows.Dialogs
{
	public class DialogButton
	{
		public string Name { get; set; } = string.Empty;

		public double Width { get; set; } = double.NaN;

		public bool IsCancel { get; set; } = false;
	}
}
