using Models.Mongo;
using System;
using System.Collections.Generic;

namespace Models.Application
{
	public class PiotroskiScore
	{
		#region Public Properties

		public long EBITDA { get; set; }
		public int FYear { get; set; }
		public DateTime LastUpdate { get; set; }
		public Dictionary<string, decimal> ProfitablityRatios { get; set; }
		public int Rating { get; set; }
		public float? Revenue { get; set; }
		public string SimId { get; set; }
		public string Ticker { get; set; }

		#endregion Public Properties
	}

	public class PiotroskiScoreMd : PiotroskiScore, IBaseModel
	{
		#region Public Properties

		public string Id { get; set; }

		#endregion Public Properties

		#region Public Constructors

		public PiotroskiScoreMd()
		{
		}

		#endregion Public Constructors
	}
}