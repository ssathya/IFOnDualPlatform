using Microsoft.Extensions.Logging;
using System;
using Utilities.StringHelpers;

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

		public EnvHandler()
		{
			_log = null;
		}

		#endregion Public Constructors


		#region Public Methods

		public string GetApiKey(string provider)
		{
			var apiKey = Environment.GetEnvironmentVariable(provider);
			if (!apiKey.IsNullOrWhiteSpace())
			{
				return apiKey;
			}
			apiKey = Environment.GetEnvironmentVariable(provider, EnvironmentVariableTarget.Process);
			if (apiKey.IsNullOrWhiteSpace())
			{
				if (_log != null)
				{
					_log.LogDebug("Did not find api key in process");
				}
				apiKey = Environment.GetEnvironmentVariable(provider, EnvironmentVariableTarget.Machine);
			}
			if (apiKey.IsNullOrWhiteSpace())
			{
				if (_log != null)
				{
					_log.LogDebug("Did not find api key in Machine");
				}
				apiKey = Environment.GetEnvironmentVariable(provider, EnvironmentVariableTarget.User);
			}
			if (apiKey.IsNullOrWhiteSpace())
			{
				if (_log != null)
				{
					_log.LogDebug("Did not find api key in User");
				}
				apiKey = Environment.GetEnvironmentVariable("NewsAPI");
			}
			if (apiKey.IsNullOrWhiteSpace())
			{
				if (_log != null)
				{
					_log.LogError($"Did not find api key for {provider}; calls will fail");
				}
			}
			return apiKey;
		}

		#endregion Public Methods
	}
}