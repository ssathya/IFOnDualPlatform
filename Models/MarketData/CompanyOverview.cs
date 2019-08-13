using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.MarketData
{
	public class CompanyOverview
    {
		public string Symbol { get; set; }
		public string CompanyName { get; set; }
		public string Exchange { get; set; }
		public string Industry { get; set; }
		public string Website { get; set; }
		public string Description { get; set; }
		public string Ceo { get; set; }
		public string IssueType { get; set; }
		public string Sector { get; set; }
		public string[] Tags { get; set; }
	}
}
