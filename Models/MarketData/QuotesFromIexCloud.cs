namespace Models.MarketData
{
	public class QuotesFromIexCloud
	{
		public string Symbol { get; set; }
		public string CompanyName { get; set; }
		public string PrimaryExchange { get; set; }
		public string CalculationPrice { get; set; }
		public float? Open { get; set; }
		public long OpenTime { get; set; }
		public float? Close { get; set; }
		public long CloseTime { get; set; }
		public float? High { get; set; }
		public float? Low { get; set; }
		public float? LatestPrice { get; set; }
		public string LatestSource { get; set; }
		public string LatestTime { get; set; }
		public long? LatestUpdate { get; set; }
		public long? LatestVolume { get; set; }
		public float? IexRealtimePrice { get; set; }
		public long? IexRealtimeSize { get; set; }
		public long? IexLastUpdated { get; set; }
		public float? DelayedPrice { get; set; }
		public long? DelayedPriceTime { get; set; }
		public float? ExtendedPrice { get; set; }
		public float? ExtendedChange { get; set; }
		public float? ExtendedChangePercent { get; set; }
		public long? ExtendedPriceTime { get; set; }
		public float? PreviousClose { get; set; }
		public long PreviousVolume { get; set; }
		public float? Change { get; set; }
		public float? ChangePercent { get; set; }
		public long Volume { get; set; }
		public float? IexMarketPercent { get; set; }
		public long? IexVolume { get; set; }
		public long? AvgTotalVolume { get; set; }
		public float? IexBidPrice { get; set; }
		public float? IexBidSize { get; set; }
		public float? IexAskPrice { get; set; }
		public float? IexAskSize { get; set; }
		public float? MarketCap { get; set; }
		public float? PeRatio { get; set; }
		public float? Week52High { get; set; }
		public float? Week52Low { get; set; }
		public float? YtdChange { get; set; }
		public long? LastTradeTime { get; set; }
		public bool IsUSMarketOpen { get; set; }
	}

}
