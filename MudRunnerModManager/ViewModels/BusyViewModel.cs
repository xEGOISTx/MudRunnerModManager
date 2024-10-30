using MudRunnerModManager.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MudRunnerModManager.ViewModels
{
	public class BusyViewModel : ViewModelBase
	{
		public event EventHandler<bool> BusyChanged;

		protected async Task BusyAction(Func<Task> action)
		{
			OnBusyChanged(true);
			try
			{
				await action();
			}
			catch(Exception ex)
			{
				await DialogManager.ShowMessageDialog(ex.Message, DialogManager.OK, AdditionalWindows.Dialogs.DialogImage.Error);
			}
			finally
			{
				OnBusyChanged(false);
			}
		}

		private void OnBusyChanged(bool isBusy)
		{
			BusyChanged?.Invoke(this, isBusy);
		}
	}
}
