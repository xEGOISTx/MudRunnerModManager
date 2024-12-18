using MudRunnerModManager.Common;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace MudRunnerModManager.ViewModels
{
	public class BusyViewModel : ViewModelBase
	{
		public event EventHandler<bool>? BusyChanged;

		protected async Task BusyAction(Func<Task> action)
		{
			OnBusyChanged(true);
			try
			{
				await action();
			}
			catch (Exception ex)
			{
				await ShowError(ex);
			}
			finally
			{
				OnBusyChanged(false);
			}		
		}

		protected async Task<BusyActionResult<T>> BusyAction<T>(Func<T> action)
		{
			OnBusyChanged(true);
			try
			{
				var res = await Task.Run(action);
				return new BusyActionResult<T> { Value = res };
			}
			catch (Exception ex)
			{
				await ShowError(ex);
			}
			finally
			{
				OnBusyChanged(false);
			}

			return new BusyActionResult<T>();
		}

		protected async Task BusyAction(Task task)
		{
			await BusyAction(() => task);
		}

		protected async Task BusyAction<T>(Func<T> action, Action<T> callback)
		{
			await BusyAction(async () =>
			{
				var res = await Task.Run(action);
				callback(res);
			});
		}

		protected async Task BusyAction(Action action, Action? callback = null)
		{
			await BusyAction(async () =>
			{
				await Task.Run(action);
				callback?.Invoke();
			});
		}

		private void OnBusyChanged(bool isBusy)
		{
			BusyChanged?.Invoke(this, isBusy);
		}
		
		private static async Task ShowError(Exception exception)
		{
			await DialogManager.ShowMessageDialog(exception.Message, DialogManager.OK, AdditionalWindows.Dialogs.DialogImage.Error);
		}
	}

	public class BusyActionResult<TValue>
	{
		public TValue? Value { get; init; } = default;

		[MemberNotNullWhen(true, nameof(Value))]
		public bool Success => Value != null;
	}
}
