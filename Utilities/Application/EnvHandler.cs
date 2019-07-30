using Microsoft.Extensions.Logging;
using System;

namespace Utilities.Application
{
	public class EnvHandler
	{

		#region Private Fields

		private readonly ILogger<EnvHandler> _log;

		#endregion Private Fields


		#region Public Constructors

		public EnvHandler(ILogger<EnvHandler> log)
		{
			_log = log;
		}

		#endregion Public Constructors


		#region Public Methods

		public string GetApiKey(string provider)
		{
			var apiKey = Environment.GetEnvironmentVariable(provider, EnvironmentVariableTarget.Process);
			if (string.IsNullOrWhiteSpace(apiKey))
			{
				_log.LogDebug("Did not find api key in process");
				apiKey = Environment.GetEnvironmentVariable(provider, EnvironmentVariableTarget.Machine);
			}
			if (string.IsNullOrWhiteSpace(apiKey))
			{
				_log.LogDebug("Did not find api key in Machine");
				apiKey = Environment.GetEnvironmentVariable(provider, EnvironmentVariableTarget.User);
			}
			if (string.IsNullOrWhiteSpace(apiKey))
			{
				_log.LogDebug("Did not find api key in Machine");
				apiKey = Environment.GetEnvironmentVariable("NewsAPI");
			}
			if (string.IsNullOrWhiteSpace(apiKey))
			{
				_log.LogError($"Did not find api key for {provider}; calls will fail");
			}
			return apiKey;
		}

		#endregion Public Methods
	}
}