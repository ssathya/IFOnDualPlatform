using System;
using System.Collections.Generic;

namespace Models.News
{
	public class Article
	{


		#region Public Properties

		public string author { get; set; }
		public string content { get; set; }
		public string description { get; set; }
		public DateTime publishedAt { get; set; }
		public Source source { get; set; }
		public string title { get; set; }
		public string url { get; set; }
		public string urlToImage { get; set; }

		#endregion Public Properties

	}

	public class NewsExtract
	{

		public NewsExtract()
		{
			articles = new List<Article>();
		}
		#region Public Properties

		public List<Article> articles { get; set; }
		public string status { get; set; }
		public int totalResults { get; set; }

		#endregion Public Properties

	}

	public class Source
	{


		#region Public Properties

		public string id { get; set; }
		public string name { get; set; }

		#endregion Public Properties

	}
}