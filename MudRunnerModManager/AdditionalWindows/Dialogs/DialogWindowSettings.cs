using Avalonia.Controls;

namespace MudRunnerModManager.AdditionalWindows.Dialogs
{
	public class DialogWindowSettings
	{
		public WindowIcon? WindowIcon { get; set; }

		public bool CanResize { get; set; }

		public string WindowTitle { get; set; } = string.Empty;

		public double MinWidth { get; set; } = 200.0;

		public double MaxWidth { get; set; } = double.PositiveInfinity;

		public double Width { get; set; } = double.NaN;

		public double MinHeight { get; set; } = 100.0;

		public double MaxHeight { get; set; } = double.PositiveInfinity;

		public double Height { get; set; } = double.NaN;

		public SizeToContent SizeToContent { get; set; } = SizeToContent.WidthAndHeight;

		public WindowStartupLocation WindowStartupLocation { get; set; }
	}
}
