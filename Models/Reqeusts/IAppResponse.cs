using System;

namespace Models.Reqeusts
{
	public interface IAppResponse
	{
		string ResponseCreatedBy { get; set; }
		DateTime ResponseCreateTime { get; set; }
		bool IsResponseSsml { get; set; }
		string ResponseData { get; set; }
	}
}
