using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Reqeusts
{
	public class CompanyNews : IAppRequest
	{
		public CompanyNews()
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
