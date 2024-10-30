using MudRunnerModManager.ViewModels;
using ReactiveUI;

namespace MudRunnerModManager.AdditionalWindows.Dialogs.TextInputDialog
{
    public class TextInputViewModel : ViewModelBase
    {
        private string _description = string.Empty;
        private string _text = string.Empty;

        public string Description
        {
            get => _description;
            set => this.RaiseAndSetIfChanged(ref _description, value, nameof(Description));
        }

        public string Text
        {
            get => _text;
            set
            {
                this.RaiseAndSetIfChanged(ref _text, value, nameof(Text));
            }
        }

    }
}
