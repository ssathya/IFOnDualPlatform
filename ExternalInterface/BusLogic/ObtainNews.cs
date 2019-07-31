using Microsoft.Extensions.Logging;
using Models.News;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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
		private const string newsAPIKey = "NewsAPI";
		private readonly ILogger<ObtainNews> _logger;
		private readonly EnvHandler _envHandler;
		private readonly string companyNews = @"https://newsapi.org/v2/everything?q=FIRM&from=SYYYY-MM-DD&to=EYYYY-MM-DD&sortBy=popularity&apiKey=API_KEY";
		public ObtainNews(ILogger<ObtainNews> logger, EnvHandler envHandler)
		{
			_logger = logger;
			_envHandler = envHandler;
		}

		public async Task<string> GetCompanyNewsAsync(string companyName)
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
			string data = "{}";
			try
			{
				using (var wc = new WebClient())
				{
					data = await wc.DownloadStringTaskAsync(urlToUse);
				}
				var newsData = JsonConvert.DeserializeObject<NewsExtract>(data);
				var returnString = new StringBuilder();
				returnString.Append($"Here are the lastest news about {companyName}");
				var publishedDate = DateTime.Now.AddYears(-1);
				foreach (var article in newsData.articles.OrderByDescending(a=>a.publishedAt))
				{
					if (article.title.IsEnglish())
					{
						if (publishedDate.Date != article.publishedAt.Date)
						{
							if (returnString.Length >= 500)
							{
								break;
							}
							publishedDate = article.publishedAt;
							var dateStr = publishedDate.ToString("MMMM dd");
							returnString.Append($". \r\nThe followin were published on {dateStr}\r\n");
						}
						returnString.Append(article.title);
						returnString.Append("\r\n");
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
	}
}
