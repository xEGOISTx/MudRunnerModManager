using ReactiveUI.Validation.Abstractions;
using ReactiveUI;
using System;

namespace MudRunnerModManager.AdditionalWindows
{
	public class ValidateConditionBase<VM>  where VM : IReactiveObject, IValidatableViewModel
	{
		public ValidateConditionBase(Func<VM, IObservable<bool>> getCondition, string message)
		{
			GetCondition = getCondition;
			Message = message;
		}

		public Func<VM, IObservable<bool>> GetCondition { get; }

		public string Message { get; }

		public DialogValidator ToValidator(VM vm)
		{
			return new DialogValidator(GetCondition(vm), Message);
		}
	}
}
