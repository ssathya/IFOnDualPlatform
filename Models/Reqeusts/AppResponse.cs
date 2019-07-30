using System;

namespace Models.Reqeusts
{
	public class AppResponse : IAppResponse
	{
		public AppResponse()
		{
			ResponseCreateTime = DateTime.Now;
			IsResponseSsml = false;
			IsResponseSuccess = false;
		}
		public string ResponseCreatedBy { get; set; }
		public DateTime ResponseCreateTime { get; set; }
		public bool IsResponseSsml { get; set; }
		public string ResponseData { get; set; }
		public bool IsResponseSuccess { get; set; }
	}
}
