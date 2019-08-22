using Microsoft.Extensions.Logging;
using Models.News;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Utilities.Application;
using Utilities.StringHelpers;

namespace ExternalInterface.BusLogic
{
	public class ObtainNews
	{


		#region Private Fields

		private readonly EnvHandler _envHandler;
		private readonly ILogger<ObtainNews> _logger;
		private readonly string companyNews = @"https://newsapi.org/v2/everything?q=FIRM&from=SYYYY-MM-DD&to=EYYYY-MM-DD&sortBy=popularity&apiKey=API_KEY";
		private readonly string mediaNews = @"https://newsapi.org/v2/top-headlines?sources=newsMedia&apiKey=API_KEY";			
		private readonly string newsAPIKey = "NewsAPI";

		#endregion Private Fields


		#region Public Constructors

		public ObtainNews(ILogger<ObtainNews> logger, EnvHandler envHandler)
		{
			_logger = logger;
			_envHandler = envHandler;
		}

		#endregion Public Constructors


		#region Public Methods		
		/// <summary>
		/// Gets a company's news asynchronous.
		/// </summary>
		/// <param name="companyName">Name of the company.</param>
		/// <returns></returns>
		public async Task<string> GetCompanyNewsAsync(string companyName)
		{
			string urlToUse = BuildCompanyNewsURL(companyName);
			string data = "{}";
			try
			{
				using (var wc = new WebClient())
				{
					data = await wc.DownloadStringTaskAsync(urlToUse);
				}
				var settings = new JsonSerializerSettings
				{
					NullValueHandling = NullValueHandling.Ignore,
					MissingMemberHandling = MissingMemberHandling.Ignore
				};
				var newsData = JsonConvert.DeserializeObject<NewsExtract>(data, settings);
				var returnString = new StringBuilder();
				returnString.Append($"Here are the latest news about {companyName}");
				var publishedDate = DateTime.Now.AddYears(-1);
				foreach (var article in newsData.articles.OrderByDescending(a => a.publishedAt))
				{
					if (article.title.IsEnglish())
					{
						if (publishedDate.Date != article.publishedAt.Date)
						{							
							publishedDate = article.publishedAt;
							var dateStr = publishedDate.ToString("MMMM dd");
							returnString.Append($". \r\nThe following were published on {dateStr}.\r\n");
						}
						returnString.Append(article.title);
						returnString.Append(".\r\n");
					}
					if (returnString.Length >= 500)
					{
						break;
					}
				}
				return returnString.ToString();
			}
			catch (Exception ex)
			{
				_logger.LogError($"Error while getting company => {companyName} news. Error details:");
				_logger.LogError(ex.Message);
				return "";
			}
		}
		/// <summary>
		/// Gets  external news.
		/// </summary>
		/// <param name="newsSource">The news source.</param>
		/// <returns></returns>
		public async Task<string> GetExternalNews(string newsSource)
		{
			var apiKey = _envHandler.GetApiKey(newsAPIKey);
			var urlToUse = mediaNews.Replace("newsMedia", newsSource.ToLower().Trim())
				.Replace("API_KEY", apiKey);
			NewsExtract extracts = await ObtainNewAPIDta(urlToUse);
			if (extracts == null)
			{
				return null;
			}
			return ExtractHeadlines(extracts);
		}

		#endregion Public Methods


		#region Private Methods

		private string BuildCompanyNewsURL(string companyName)
		{
			var apiKey = _envHandler.GetApiKey(newsAPIKey);
			var today = DateTime.Now.ToString("yyyy-MM-dd");
			var tenDaysAgo = DateTime.Now.AddDays(-10).ToString("yyyy-MM-dd");
			var firm = Uri.EscapeDataString(companyName);
			var urlToUse = companyNews.Replace("FIRM", firm)
				.Replace("SYYYY-MM-DD", tenDaysAgo)
				.Replace("EYYYY-MM-DD", today);
			_logger.LogDebug($"Url used to get {companyName} news is \n\t{urlToUse}");
			urlToUse = urlToUse.Replace("API_KEY", apiKey);
			return urlToUse;
		}
		private string ExtractHeadlines(NewsExtract extracts)
		{
			var returnMsg = new StringBuilder();
			if (extracts.totalResults == 0 || !extracts.status.Equals("ok"))
			{
				return "Cannot obtain news at this time\n";
			}
			returnMsg.Append("Here are the headlines from " + extracts.articles[0].source.name + ".\n");
			foreach (var article in extracts.articles)
			{
				returnMsg.Append(article.title + ".\n");
			}
			return returnMsg.ToString().ConvertAllToASCII();
		}
		private async Task<NewsExtract> ObtainNewAPIDta(string urlToUse)
		{
			string data = "{}";
			try
			{
				using (var wc = new WebClient())
				{
					data = await wc.DownloadStringTaskAsync(urlToUse);
				}
				var settings = new JsonSerializerSettings
				{
					NullValueHandling = NullValueHandling.Ignore,
					MissingMemberHandling = MissingMemberHandling.Ignore
				};
				var newsData = JsonConvert.DeserializeObject<NewsExtract>(data, settings);
				return newsData;
			}
			catch (Exception ex)
			{
				_logger.LogCritical($"Error while obtaining news\n{ex.Message}");
				return null;
			}
		}

		#endregion Private Methods

	}
}
