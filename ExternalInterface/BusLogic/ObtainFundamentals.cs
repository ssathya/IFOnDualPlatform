using AutoMapper;
using Microsoft.Extensions.Logging;
using Models.Application;
using Models.MarketData;
using MongoHandler;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Utilities.Application;
using Utilities.StringHelpers;

namespace ExternalInterface.BusLogic
{
	public class ObtainFundamentals
	{

		#region Private Fields

		private const string apiKey = "{api-key}";
		private const string iexAnalystRatings = @"https://cloud.iexapis.com/stable/stock/{ticker}/recommendation-trends?token={api-key}";
		private const string iexTradingProvider = "IEXTrading";
		private readonly IDBConnectionHandler<PiotroskiScoreMd> _connectionHandlerCF;
		private readonly EnvHandler _envHandler;
		private readonly ILogger<ObtainFundamentals> _logger;
		private readonly IMapper _mapper;
		private readonly ResolveCompanyName _resolveCompanyName;
		private readonly string iexCompanyDetailsURL = @"https://cloud.iexapis.com/stable/stock/{ticker}/company?token={api-key}";
		private readonly string iexCompanyStatssURL = @"https://cloud.iexapis.com/stable/stock/{ticker}/stats?token={api-key}";

		#endregion Private Fields

		#region Public Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="ObtainFundamentals"/> class.
		/// </summary>
		/// <param name="logger">The logger.</param>
		/// <param name="resolveCompanyName">Name of the resolve company.</param>
		/// <param name="envHandler">The env handler.</param>
		/// <param name="connectionHandlerCF">The connection handler cf.</param>
		/// <param name="mapper">The mapper.</param>
		public ObtainFundamentals(ILogger<ObtainFundamentals> logger, ResolveCompanyName resolveCompanyName, EnvHandler envHandler, IDBConnectionHandler<PiotroskiScoreMd> connectionHandlerCF, IMapper mapper)
		{
			_logger = logger;
			_resolveCompanyName = resolveCompanyName;
			_envHandler = envHandler;
			_connectionHandlerCF = connectionHandlerCF;
			_mapper = mapper;
			_connectionHandlerCF.ConnectToDatabase("PiotroskiScore");
		}

		#endregion Public Constructors


		#region Public Methods

		public async Task<string> GetCompanyFundamentals(string companyName)
		{
			_logger.LogTrace("Starting fundamental details");
			var tickersToUse = await _resolveCompanyName.ResolveCompanyNameOrTicker(companyName);
			if (tickersToUse.IsNullOrWhiteSpace())
			{
				return "";
			}
			var fulfillmentText = new StringBuilder();

			try
			{
				int counter = 0;
				foreach (var ticker in tickersToUse.Split(','))
				{
					var ratingsMd = _connectionHandlerCF.Get().Where(x => x.Ticker.ToLower().Equals(ticker.ToLower()))
						.OrderByDescending(x => x.FYear).FirstOrDefault();
					PiotroskiScore computedRating = null;

					if (ratingsMd != null && DateTime.Now.Year - ratingsMd.FYear <= 2)
					{
						computedRating = _mapper.Map<PiotroskiScore>(ratingsMd);
					}
					fulfillmentText.Append(await BuildCompanyProfile(ticker, computedRating));
					if (++counter >= 2)
					{
						break;
					}
				}
				if (counter >= 2)
				{
					fulfillmentText.Append($"Limiting result set as the search term {companyName} resolved to too many results.\n");
				}
			}
			catch (Exception ex)
			{
				_logger.LogCritical($"Error while processing Get Company Fundamentals; \n{ex.Message}");
				return "";
			}
			if (fulfillmentText.ToString().Contains("Piotroski"))
			{
				fulfillmentText.Append("\nNote: Piotroski F-Score is based on company fundamentals; " +
					"a rating greater than 6 indicates strong fundamentals.");
			}
			return fulfillmentText.ToString();
		}

		#endregion Public Methods


		#region Private Methods

		private async Task<string> BuildAnalystsRatings(string ticker)
		{
			var urlToUseForAnalystRating = iexAnalystRatings.Replace(@"{ticker}", ticker)
				.Replace(@"{api-key}", _envHandler.GetApiKey(iexTradingProvider));
			try
			{
				using (var wc = new WebClient())
				{
					string data = "[]";
					data = await wc.DownloadStringTaskAsync(urlToUseForAnalystRating);
					var settings = new JsonSerializerSettings
					{
						NullValueHandling = NullValueHandling.Ignore,
						MissingMemberHandling = MissingMemberHandling.Ignore
					};
					var ratingsArray = JsonConvert.DeserializeObject<IEnumerable<Ratings>>(data,settings).ToList();
					if (ratingsArray == null || !ratingsArray.Any())
					{
						return "";
					}
					var ratingsToUse = ratingsArray.OrderBy(r => r.ConsensusEndDate).FirstOrDefault();
					var totalRatings = ratingsToUse.RatingBuy + ratingsToUse.RatingOverweight
						+ ratingsToUse.RatingHold + ratingsToUse.RatingUnderweight
						+ ratingsToUse.RatingSell + ratingsToUse.RatingNone;
					if (totalRatings == 0)
					{
						return "";
					}
					StringBuilder returnString = BuildRatingsMessage(ratingsToUse, totalRatings, ticker);
					return returnString.ToString();
				}
			}
			catch (Exception ex)
			{
				_logger.LogError($"Error in BuildAnalystsRatings; message\n{ex.Message}\n ");
				return "";
			}
		}

		private string BuildCommonStockMessage(PiotroskiScore computedRating, string symbol)
		{
			var returnText = new StringBuilder();
			if (computedRating == null)
			{
				returnText.Append($" We do not have any additional details about {symbol} at this time.\n");
				return returnText.ToString();
			}
			returnText.Append($"Additional details about {symbol} using its SEC filings.\n");
			returnText.Append($" Revenues {((decimal)computedRating.Revenue).ToKMB()}.\n");
			returnText.Append($" EBITDA {((decimal)(computedRating.EBITDA)).ToKMB()}. \n");
			returnText.Append($" Gross Margin {computedRating.ProfitablityRatios["Gross Margin"]}%.\n");
			returnText.Append($" Operating Margin {computedRating.ProfitablityRatios["Operating Margin"]}%.\n");
			returnText.Append($" Net Profit Margin {computedRating.ProfitablityRatios["Net Profit Margin"]}%.\n");
			returnText.Append($" Return on Equity {computedRating.ProfitablityRatios["Return on Equity"]}%.\n");
			returnText.Append($" Return on Assets {computedRating.ProfitablityRatios["Return on Assets"]}%.\n");
			if (computedRating.Rating != 0)
			{
				returnText.Append($" Piotroski F-Score ratings for {symbol} is {computedRating.Rating}.  \n");
			}
			return returnText.ToString();
		}

		private async Task<string> BuildCompanyProfile(string ticker, PiotroskiScore computedRating)
		{
			var symbol = Regex.Replace(ticker, ".{1}", "$0 ");
			var companyOverview = await ObtainCompanyOverview(ticker);
			var companyStats = await ObtainCompanyStats(ticker);
			companyStats.ttmDividendRate = companyStats.ttmDividendRate == null ? 0 :
				companyStats.ttmDividendRate;
			companyStats.ttmEPS = companyStats.ttmEPS == null ? 0 :
				companyStats.ttmEPS;
			var returnText = new StringBuilder();
			if (companyOverview == null || string.IsNullOrWhiteSpace(companyOverview.Symbol))
			{
				returnText.Append($" No basic information about {symbol} found at this time\n");
			}
			else
			{
				returnText.Append($"Basic information about {companyOverview.CompanyName} trading with symbol {symbol}.\n");
				returnText.Append($" {(companyOverview.Description.IsNullOrWhiteSpace() ? "This information is not available now" : companyOverview.Description.TruncateAtWord(175))}\n");
				if (companyOverview.Description.Length >= 175)
				{
					returnText.Append("\n\n..... more removed. ....\n\n");
				}
				returnText.Append($" Its industry sector is {companyOverview.Sector} and falls under {companyOverview.Industry}.\n\n ");
				returnText.Append($" {companyOverview.CompanyName} has a market cap of {((decimal)companyStats.marketcap).ToKMB()}, " +
					$" TTM E P S of about {((decimal)companyStats.ttmEPS).ToKMB()}, and  divident rate around {((decimal)companyStats.ttmDividendRate).ToKMB()}.\n");
			}
			switch (companyOverview.IssueType.ToLower())
			{
				case "cs":
					returnText.Append(BuildCommonStockMessage(computedRating, symbol));
					//1returnText.Append(await BuildAnalystsRatings(ticker));
					break;

				case "ad":
					returnText.Append($" {symbol} is an American Depositary Receipt: ADR");
					break;

				case "re":
					returnText.Append($" {symbol} is a Real estate investment trust: REIT");
					break;

				case "ce":
				case "cef":
					returnText.Append($" {symbol} is a closed end fund");
					break;

				case "si":
					returnText.Append($" {symbol} is a secondary issue");
					break;

				case "et":
				case "etf":
					returnText.Append($" {symbol} is a exchange traded fund; ETF");
					break;

				case "ps":
				case "wt":
					returnText.Append($" {symbol} is a preferred stock or warrant; individuals do not trade these!");
					break;

				default:
					returnText.Append($" No additional information is available for {symbol}");
					break;
			}
			return returnText.ToString();
		}

		private StringBuilder BuildRatingsMessage(Ratings ratingsToUse, int totalRatings, string symbol)
		{
			var returnString = new StringBuilder();
			returnString.Append($"\n\nOut of {totalRatings} Analysts ratings {symbol} has ");
			returnString.Append(ratingsToUse.RatingBuy != 0 ?
				$" {ratingsToUse.RatingBuy} Buy " : "");
			returnString.Append(ratingsToUse.RatingOverweight != 0 ?
				$" {ratingsToUse.RatingOverweight} Overweight " : "");
			returnString.Append(ratingsToUse.RatingHold != 0 ?
				$" {ratingsToUse.RatingHold} Hold " : "");
			returnString.Append(ratingsToUse.RatingUnderweight != 0 ?
				$" {ratingsToUse.RatingUnderweight} Underweight " : "");
			returnString.Append(ratingsToUse.RatingSell != 0 ?
				$" {ratingsToUse.RatingSell} Sell " : "");
			returnString.Append(ratingsToUse.RatingNone != 0 ?
				$" {ratingsToUse.RatingNone} Neutral " : "");
			returnString.Append($" with an average rating of {ratingsToUse.RatingScaleMark.ToString("n1")}.\n\n 1 being strong buy 5 is a strong sell.\n\n");
			return returnString;
		}

		private async Task<CompanyOverview> ObtainCompanyOverview(string ticker)
		{
			var urlToUse = iexCompanyDetailsURL.Replace("{ticker}", ticker)
				.Replace(apiKey, _envHandler.GetApiKey(iexTradingProvider));
			try
			{
				using (var wc = new WebClient())
				{
					string data = "{}";
					data = await wc.DownloadStringTaskAsync(urlToUse);
					var settings = new JsonSerializerSettings
					{
						NullValueHandling = NullValueHandling.Ignore,
						MissingMemberHandling = MissingMemberHandling.Ignore
					};
					var companyOverview = JsonConvert.DeserializeObject<CompanyOverview>(data,settings);
					return companyOverview;
				}
			}
			catch (Exception ex)
			{
				_logger.LogError("Error while getting data from IEX Trading");
				_logger.LogError(ex.Message);
				if (ex.InnerException != null)
				{
					_logger.LogError(ex.InnerException.Message);
				}
				return new CompanyOverview();
			}
		}

		private async Task<CompanyKeyStats> ObtainCompanyStats(string ticker)
		{
			var urlToUse = iexCompanyStatssURL.Replace("{ticker}", ticker)
				.Replace(apiKey, _envHandler.GetApiKey(iexTradingProvider));
			try
			{
				using (var wc = new WebClient())
				{
					string data = "{}";
					data = await wc.DownloadStringTaskAsync(urlToUse);
					var settings = new JsonSerializerSettings
					{
						NullValueHandling = NullValueHandling.Ignore,
						MissingMemberHandling = MissingMemberHandling.Ignore
					};
					var companyOverview = JsonConvert.DeserializeObject<CompanyKeyStats>(data, settings);
					return companyOverview;
				}
			}
			catch (Exception ex)
			{
				_logger.LogError("Error while getting data from IEX Trading");
				_logger.LogError(ex.Message);
				if (ex.InnerException != null)
				{
					_logger.LogError(ex.InnerException.Message);
				}
				return new CompanyKeyStats();
			}
		}

		#endregion Private Methods
	}
}