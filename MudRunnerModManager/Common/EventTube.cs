using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MudRunnerModManager.Common
{
	public delegate void TubeEventHandler(object sender, EventArgs e, EventKey key);
	public enum EventKey
	{
		SettingsChanged
	}

	public static class EventTube
	{
		public static event TubeEventHandler EventPushed;

		public static void PushEvent(object sender, EventArgs e, EventKey key)
		{
			EventPushed?.Invoke(sender, e, key);
		}
	}
}
