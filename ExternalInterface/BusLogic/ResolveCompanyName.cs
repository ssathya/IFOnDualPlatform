using Microsoft.Extensions.Logging;
using Models.Application;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Utilities.Application;

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



		public ResolveCompanyName(ILogger<ResolveCompanyName> logger,  EnvHandler envHandler)
		{
			_logger = logger;
			_envHandler = envHandler;
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
				return "";
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
			var urlToUse = iexSymbolListURL.Replace(apiKey, _envHandler.GetApiKey(iexTradingProvider));
			var url1ToUse = iexOTCSymbolListURL.Replace(apiKey, _envHandler.GetApiKey(iexTradingProvider));
			try
			{
				using (var wc = new WebClient())
				{
					string data = "{}";
					data = await wc.DownloadStringTaskAsync(urlToUse);
					symbols = JsonConvert.DeserializeObject<IEnumerable<SecuritySymbol>>(data).ToList();
					if (symbols.Any())
					{
						data = "{}";
						data = await wc.DownloadStringTaskAsync(url1ToUse);
						var symbols1 = JsonConvert.DeserializeObject<IEnumerable<SecuritySymbol>>(data).ToList();
						symbols.AddRange(symbols1);
					}
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
