using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MudRunnerModManager.AdditionalWindows
{
	public class DialogValidator
	{
		public DialogValidator(IObservable<bool> rule, string message)
		{
			Message = message;
			Rule = rule;
		}

		public string Message { get; }

		public IObservable<bool> Rule { get; }
	}
}
