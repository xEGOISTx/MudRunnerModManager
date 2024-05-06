using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MudRunnerModManager.AdditionalWindows.SelectItemDialog
{
	public class SelectItemBox
	{
		private readonly string _description;
		private readonly IEnumerable<string> _items;
		private readonly IEnumerable<SelectItemValidateCondition>? _validateConditions;

		public SelectItemBox(IEnumerable<string> items, 
			string description, 
			IEnumerable<SelectItemValidateCondition>? validateConditions = null)
		{
			_items = items;
			_description = description;
			_validateConditions = validateConditions;
		}


		public async Task<SelectItemBoxResult> ShowDialog(Window owner)
		{
			if (owner == null)
			{
				throw new ArgumentNullException(nameof(owner));
			}

			SelectItemWindow view = new();
			SelectItemViewModel viewModel = new()
			{
				Description = _description,
				Items = new List<string>(_items),
				SelectedItem = _items.FirstOrDefault()
			};

			if(_validateConditions != null)
			{
				viewModel.SetValidator(_validateConditions.Select(cond => cond.ToValidator(viewModel)));
			}

			view.WindowStartupLocation = WindowStartupLocation.CenterOwner;
			view.DataContext = viewModel;

			SelectItemBoxResult res = new();
			void Closing(object? sender, WindowClosingEventArgs e)
			{
				res = new SelectItemBoxResult { OK = view.OK, SelectedItem = viewModel.SelectedItem };
			}

			view.Closing += Closing;
			await view.ShowDialog(owner);
			view.Closing -= Closing;

			return res;
		}
	}


	public class SelectItemValidateCondition : ValidateConditionBase<SelectItemViewModel>
	{
		public SelectItemValidateCondition(Func<SelectItemViewModel, IObservable<bool>> getCondition, string message)
			: base(getCondition, message)
		{

		}
	}


	public class SelectItemBoxResult
	{
		public bool OK { get; init; } = false;

		public string? SelectedItem { get; init; } = string.Empty;
	}
}
