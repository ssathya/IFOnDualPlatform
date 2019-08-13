using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.MarketData
{
	public class CompanyKeyStats
	{

		public float week52change { get; set; }
		public float week52high { get; set; }
		public float week52low { get; set; }
		public long marketcap { get; set; }
		public int? employees { get; set; }
		public float day200MovingAvg { get; set; }
		public float day50MovingAvg { get; set; }
		public float? _float { get; set; }
		public float avg10Volume { get; set; }
		public float avg30Volume { get; set; }
		public float? ttmEPS { get; set; }
		public float? ttmDividendRate { get; set; }
		public string companyName { get; set; }
		public long sharesOutstanding { get; set; }
		public float? maxChangePercent { get; set; }
		public float? year5ChangePercent { get; set; }
		public float? year2ChangePercent { get; set; }
		public float? year1ChangePercent { get; set; }
		public float? ytdChangePercent { get; set; }
		public float? month6ChangePercent { get; set; }
		public float? month3ChangePercent { get; set; }
		public float? month1ChangePercent { get; set; }
		public float? day30ChangePercent { get; set; }
		public float? day5ChangePercent { get; set; }
		public string nextDividendDate { get; set; }
		public float? dividendYield { get; set; }
		public string nextEarningsDate { get; set; }
		public string exDividendDate { get; set; }
		public float? peRatio { get; set; }
		public float? beta { get; set; }
	}	
}
