using Microsoft.Extensions.Logging;
using Models.Application;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Utilities.Application;
using Utilities.StringHelpers;

namespace ExternalInterface.BusLogic
{
	public class ResolveCompanyName
	{
		private readonly ILogger<ResolveCompanyName> _logger;
		private readonly EnvHandler _envHandler;
		private static List<SecuritySymbol> symbols;
		private const string iexTradingProvider = "IEXTrading";
		private const string apiKey = "{api-key}";
		private static DateTime lastSymbolUpdate;
		private readonly string iexSymbolListURL = @"https://cloud.iexapis.com/stable/ref-data/symbols?token={api-key}";
		private readonly string iexOTCSymbolListURL = @"https://cloud.iexapis.com/stable/ref-data/otc/symbols?token={api-key}";



		public ResolveCompanyName(ILogger<ResolveCompanyName> logger, EnvHandler envHandler)
		{
			_logger = logger;
			_envHandler = envHandler;
		}
		public async Task<string> ResolveTicker(string ticker)
		{
			symbols = await ObtainSymbolsFromIeXAsync();
			_logger.LogTrace($"Trying to resolve {ticker}");
			if (symbols == null || symbols.Count <= 5)
			{
				return "";
			}
			var companyName = symbols.Find(t => t.Symbol.Equals(ticker)).Name;
			return companyName.IsNullOrWhiteSpace() ? "" : companyName;

		}
		public async Task<string> ResolveCompanyNameOrTicker(string companyName)
		{
			symbols = await ObtainSymbolsFromIeXAsync();
			_logger.LogTrace($"Trying to resolve {companyName}");
			if (symbols == null || symbols.Count <= 5)
			{
				return "";
			}
			var tick = new List<string>();

			var localCompanyName = companyName.ToLower();
			var tickerSearch = (from s in symbols
								where s.Symbol.ToLower().Equals(localCompanyName)
								select s.Symbol).FirstOrDefault();
			if (!string.IsNullOrWhiteSpace(tickerSearch))
			{
				tick.Add(tickerSearch);
			}
			else
			{
				tick.AddRange((from s in symbols
							   where s.Name.ToLower().StartsWith(companyName.ToLower())
							   && (s.Type.ToLower() == "cs" || s.Type.ToLower().Contains("et"))
							   select s.Symbol).ToList());
				if (!tick.Any())
				{
					tick.AddRange((from s in symbols
								   where s.Name.ToLower().Contains(companyName.ToLower())
								   && (s.Type.ToLower() == "cs" || s.Type.ToLower().Contains("et"))
								   select s.Symbol).ToList());
				}
				if (!tick.Any())
				{
					tick.AddRange((from s in symbols
								   where s.Name.ToLower().Contains(companyName.ToLower())
								   select s.Symbol).ToList());
				}
				tick = tick.Distinct().Take(5).ToList();
			}
			if (!tick.Any())
			{
				//Try replacing " and " with " & "
				if (localCompanyName.ToLower().Contains(" and "))
				{
					var newLocalCompanyName = localCompanyName.Replace(" and ", " & ");
					var returnValue = await ResolveCompanyNameOrTicker(newLocalCompanyName);
					if (!returnValue.IsNullOrWhiteSpace())
					{
						return returnValue;
					}
					newLocalCompanyName = localCompanyName.Replace(" and ", "&");
					returnValue = await ResolveCompanyNameOrTicker(newLocalCompanyName);
					if (!returnValue.IsNullOrWhiteSpace())
					{
						return returnValue;
					}
				}
				else if (localCompanyName.Count(f => f == ' ') == 1) //handle sales force to Salesforce
				{
					var newLocalCompanyName = localCompanyName.Replace(" and ", " & ");
					var returnValue = await ResolveCompanyNameOrTicker(newLocalCompanyName);
					return returnValue;
				}
				else
				{
					return "";
				}
			}
			string returnString = tick.Aggregate((i, j) => i + "," + j);
			_logger.LogTrace($"Company name {companyName} was resolved as {returnString}");
			return returnString;
		}
		private async Task<List<SecuritySymbol>> ObtainSymbolsFromIeXAsync()
		{
			if (symbols != null && symbols.Count >= 5 && (DateTime.Now - lastSymbolUpdate).TotalDays < 1)
			{
				return symbols;
			}
			if (symbols == null)
			{
				symbols = new List<SecuritySymbol>();
			}
			else
			{
				symbols.Clear();
			}
			var urlToUse = iexSymbolListURL.Replace(apiKey, _envHandler.GetApiKey(iexTradingProvider));
			var url1ToUse = iexOTCSymbolListURL.Replace(apiKey, _envHandler.GetApiKey(iexTradingProvider));
			try
			{
				using (var httpClient = new HttpClient())
				{
					string data = "{}";
					data = await httpClient.GetStringAsync(urlToUse);
					var settings = new JsonSerializerSettings
					{
						NullValueHandling = NullValueHandling.Ignore,
						MissingMemberHandling = MissingMemberHandling.Ignore
					};
					var symbols0 = JsonConvert.DeserializeObject<IEnumerable<SecuritySymbol>>(data,settings).ToList();
					symbols.AddRange(symbols0);
					var data1 = "{}";
					data1 = await httpClient.GetStringAsync(url1ToUse);
					var symbols1 = JsonConvert.DeserializeObject<IEnumerable<SecuritySymbol>>(data1, settings).ToList();
					symbols.AddRange(symbols1);
					lastSymbolUpdate = DateTime.Now;
					return symbols;
				}
			}
			catch (Exception ex)
			{
				_logger.LogError("Error while getting data from IEX trading");
				_logger.LogError(ex.Message);
				return new List<SecuritySymbol>();
			}
		}
	}
}
