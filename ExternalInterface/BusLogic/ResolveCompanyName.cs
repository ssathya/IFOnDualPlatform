using AutoMapper;
using Microsoft.Extensions.Logging;
using Models.Application;
using MongoHandler;
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

		#region Private Fields

		private const string apiKey = "{api-key}";
		private const string iexTradingProvider = "IEXTrading";
		private const string SecSymobls = "SecuritySymbol";
		private static DateTime lastSymbolUpdate;
		private static List<SecuritySymbol> symbols;
		private readonly IDBConnectionHandler<LastUpateCollectionMd> _connectionHandlerLU;
		private readonly IDBConnectionHandler<SecuritySymbolMd> _connectionHandlerSS;
		private readonly EnvHandler _envHandler;
		private readonly ILogger<ResolveCompanyName> _logger;
		private readonly IMapper _mapper;
		private readonly string iexOTCSymbolListURL = @"https://cloud.iexapis.com/stable/ref-data/otc/symbols?token={api-key}";
		private readonly string iexSymbolListURL = @"https://cloud.iexapis.com/stable/ref-data/symbols?token={api-key}";

		#endregion Private Fields


		#region Public Constructors

		public ResolveCompanyName(ILogger<ResolveCompanyName> logger, EnvHandler envHandler,
			IDBConnectionHandler<SecuritySymbolMd> connectionHandlerSS,
			IDBConnectionHandler<LastUpateCollectionMd> connectionHandlerLU,
			IMapper mapper)
		{
			_logger = logger;
			_envHandler = envHandler;
			_connectionHandlerSS = connectionHandlerSS;
			_connectionHandlerLU = connectionHandlerLU;
			_mapper = mapper;
			_connectionHandlerSS.ConnectToDatabase("SecuritySymbol");
			_connectionHandlerLU.ConnectToDatabase("LastUpdate");
		}

		#endregion Public Constructors


		#region Public Methods

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
				{     //Lowe's
					tick.AddRange((from s in symbols
								   where s.Name.ToLower().RemovePunctuations().StartsWith(companyName.ToLower())
								   && (s.Type.ToLower() == "cs" || s.Type.ToLower().Contains("et"))
								   select s.Symbol).ToList());
				}
				if (!tick.Any())
				{
					tick.AddRange((from s in symbols
								   where s.Name.ToLower().Contains(companyName.ToLower())
								   && (s.Type.ToLower() == "cs" || s.Type.ToLower().Contains("et"))
								   select s.Symbol).ToList());
				}
				if (!tick.Any())
				{ //The Container Company => Container Company
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
					//bed bath and beyond
					var newLocalCompanyName = localCompanyName.Replace(" and ", " & ");
					var returnValue = await ResolveCompanyNameOrTicker(newLocalCompanyName);
					if (!returnValue.IsNullOrWhiteSpace())
					{
						return returnValue;
					}
					//s and p => s&p
					newLocalCompanyName = localCompanyName.Replace(" and ", "&");
					returnValue = await ResolveCompanyNameOrTicker(newLocalCompanyName);
					if (!returnValue.IsNullOrWhiteSpace())
					{
						return returnValue;
					}
				}
				else if (localCompanyName.Count(f => f == ' ') == 1) //handle sales force to Salesforce
				{
					var newLocalCompanyName = localCompanyName.Replace(" ", "");
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

		#endregion Public Methods


		#region Private Methods

		private LastUpateCollectionMd GetSymbolsFromDb()
		{
			LastUpateCollectionMd lastUpdateDate = _connectionHandlerLU.Get(r => r.CollectionName.Equals(SecSymobls)).FirstOrDefault();
			if (lastUpdateDate != null && lastUpdateDate.LastUpdate >= DateTime.Now.AddDays(-1))
			{
				var symbolsMd = _connectionHandlerSS.Get().ToList();
				if (symbolsMd != null && symbolsMd.Any())
				{
					symbols = _mapper.Map<List<SecuritySymbol>>(symbolsMd);
				}
			}
			return lastUpdateDate;
		}

		private async Task<List<SecuritySymbol>> ObtainSymbolsFromIeXAsync()
		{
			LastUpateCollectionMd lastUpdateDate = null;
			if (symbols != null && symbols.Count >= 100 && (DateTime.Now - lastSymbolUpdate).TotalDays < 1)
			{
				return symbols;
			}

			lastUpdateDate = GetSymbolsFromDb();
			if (symbols != null && symbols.Count >= 100)
			{
				lastSymbolUpdate = lastUpdateDate.LastUpdate;
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
					var symbols0 = JsonConvert.DeserializeObject<IEnumerable<SecuritySymbol>>(data, settings).ToList();
					symbols.AddRange(symbols0);
					var data1 = "{}";
					data1 = await httpClient.GetStringAsync(url1ToUse);
					var symbols1 = JsonConvert.DeserializeObject<IEnumerable<SecuritySymbol>>(data1, settings).ToList();
					symbols.AddRange(symbols1);
					lastSymbolUpdate = DateTime.Now;
					lastUpdateDate = await PopulateSymbolsToDbAsync(lastUpdateDate);
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
		private async Task<LastUpateCollectionMd> PopulateSymbolsToDbAsync(LastUpateCollectionMd lastUpdateDate)
		{
			var symbolsMd = _mapper.Map<List<SecuritySymbolMd>>(symbols);
			var updateResult = false;
			if (lastUpdateDate == null)
			{
				lastUpdateDate = new LastUpateCollectionMd
				{
					CollectionName = SecSymobls,
					LastUpdate = DateTime.Now
				};
				lastUpdateDate = await _connectionHandlerLU.Create(lastUpdateDate);
				if (lastUpdateDate.Id.IsNullOrWhiteSpace())
				{
					_logger.LogCritical("Could not insert record to collection LastUpdate");
				}
			}
			else
			{
				lastUpdateDate.LastUpdate = DateTime.Now;
				updateResult = await _connectionHandlerLU.Update(lastUpdateDate.Id, lastUpdateDate);
				if (!updateResult)
				{
					_logger.LogCritical("Could not update record in LastUpdate");
				}
			}
			updateResult = await _connectionHandlerSS.RemoveAll();
			if (!updateResult)
			{
				_logger.LogCritical("Could delete all records in SecuritySymbol");
			}
			updateResult = await _connectionHandlerSS.Create(symbolsMd);
			if (!updateResult)
			{
				_logger.LogCritical("Could insert records in SecuritySymbol");
			}
			return lastUpdateDate;
		}

		#endregion Private Methods
	}
}