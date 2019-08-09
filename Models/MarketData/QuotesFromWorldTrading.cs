using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.MarketData
{
	public class QuotesFromWorldTrading
	{
		public int Symbols_requested { get; set; }
		public int Symbols_returned { get; set; }
		public Datum[] Data { get; set; }
	}

	public class Datum
	{
		public string Symbol { get; set; }
		public string Name { get; set; }
		public string Currency { get; set; }
		public float? Price { get; set; }
		public float? Price_open { get; set; }
		public float? Day_high { get; set; }
		public float? Day_low { get; set; }
		public string _52_week_high { get; set; }
		public string _52_week_low { get; set; }
		public float? Day_change { get; set; }
		public float? Change_pct { get; set; }
		public float? Close_yesterday { get; set; }
		public string Market_cap { get; set; }
		public string Volume { get; set; }
		public string Volume_avg { get; set; }
		public string Shares { get; set; }
		public string Stock_exchange_long { get; set; }
		public string Stock_exchange_short { get; set; }
		public string Timezone { get; set; }
		public string Timezone_name { get; set; }
		public string Gmt_offset { get; set; }
		public DateTime? Last_trade_time { get; set; }
	}
}
