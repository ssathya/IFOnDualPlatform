using System;

namespace Models.Reqeusts
{
	public class CompanyData : IAppRequest
	{
		public CompanyData()
		{
			RequestStart = DateTime.Now;
			IsSsmlResponseRequested = false;
		}

		public string RequestName { get; set; }
		public DateTime RequestStart { get; set; }
		public string CompanyName { get; set; }
		public bool IsSsmlResponseRequested { get; set; }
	}
}