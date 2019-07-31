using System;

namespace Models.Reqeusts
{
	public interface IAppRequest
	{
		string RequestName { get; set; }
		DateTime RequestStart { get; set; }
		
		bool IsSsmlResponseRequested { get; set; }
	}
}
