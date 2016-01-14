using System;
using System.Threading;

namespace Clide
{
	public static class Retry
	{
		public static void Try (Action action, Func<bool> condition, int retries = 5, int interval = 200, bool throwOnTimeout = false)
		{
			var count = 0;
			var sleep = interval;
			while (count++ < retries && condition ()) {
				try {
					action ();
					return;
				} catch {
					Thread.Sleep (sleep);
					// Make the interval exponential to increase chances of suceeding
					sleep = sleep * 2;
				}
			}

			if (throwOnTimeout)
				throw new TimeoutException ();
		}
	}
}
