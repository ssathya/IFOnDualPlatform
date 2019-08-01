using System;

namespace Models.Reqeusts
{
	public class StandardNews : IAppRequest
	{
		public StandardNews()
		{
			RequestStart = DateTime.Now;
			IsSsmlResponseRequested = false;
		}

		public string RequestName { get; set; }
		public DateTime RequestStart { get; set; }
		public bool IsSsmlResponseRequested { get; set; }
		public string NewsSource { get; set; }
	}
}