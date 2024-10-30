namespace MudRunnerModManager.AdditionalWindows.Dialogs.MsgDialog
{
	public class MsgBox : BaseBox<string>
	{
		private readonly DialogImage _img;
		private readonly string _message;

		public MsgBox(string message, 
			DialogButton[] buttons, 
			DialogImage msgImage = DialogImage.None, 
			DialogWindowSettings? dialogWindowSettings = null) 
			: base(buttons, dialogWindowSettings ?? new DialogWindowSettings())
		{ 
			_message = message;
			_img = msgImage;
		}

		protected override BoxContent GetBoxContent()
		{
			return new BoxContent(_message)
			{
				DialogImage = _img,
			};
		}

		protected override string GetResult(string buttonResult, BoxContent content)
		{
			return buttonResult;
		}
	}
}
