using SpeechMaster.Services.Engines;
using SpeechMaster.Models;

namespace SpeechMaster.Services
{
	/// <summary>
	/// Manages ASR engines within the application.
	/// Provides information about available engines 
	/// and handles their initialization and configuration.
	/// </summary>
	public class EngineManager
    {
		private List<AsrEngine> _engines = new();

		public List<AsrEngine> AvailableEngines
		{
			get { return _engines = new(); }
			protected set { _engines = value; }
		}

		public EngineManager()
		{
			_engines.AddRange(
			[
				new WhisperEngine(),
			]);
		}
	}
}
