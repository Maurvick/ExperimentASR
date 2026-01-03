using System;
using System.Collections.Generic;
using System.Text;

namespace ExperimentASR.Services
{
    public class StatusService
    {
		// Singleton instance
		private static StatusService? _instance;
		public static StatusService Instance => _instance ??= new StatusService();
		// Subscribe to this event to get status updates
		public event Action<string>? OnStatusChanged;

		private StatusService() { }

		public void Update(string message)
		{
			// Invoke the event to notify subscribers
			OnStatusChanged?.Invoke(message);
		}
	}
}
