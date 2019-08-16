using System;

namespace Models.Reqeusts
{
	public class MarketData : IAppRequest
	{
		public MarketData()
		{
			RequestStart = DateTime.Now;
			IsSsmlResponseRequested = false;
			RequestName = "Market Data";
		}
		public string RequestName { get; set; }
		public DateTime RequestStart { get; set; }
		public bool IsSsmlResponseRequested { get; set; }
	}
	public class Recommendations : IAppRequest
	{
		public Recommendations()
		{
			RequestName = "Recommendations";
			RequestStart = DateTime.Now;
			IsSsmlResponseRequested = false;
		}
		public string RequestName { get; set; }
		public DateTime RequestStart { get; set; }
		public bool IsSsmlResponseRequested { get; set; }
	}
}