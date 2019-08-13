namespace Models.MarketData
{
	public class Ratings
	{
		#region Public Properties

		public long? ConsensusEndDate { get; set; }
		public long? ConsensusStartDate { get; set; }
		public long? CorporateActionsAppliedDate { get; set; }
		public int RatingBuy { get; set; }
		public int RatingHold { get; set; }
		public int RatingNone { get; set; }
		public int RatingOverweight { get; set; }
		public float RatingScaleMark { get; set; }
		public int RatingSell { get; set; }
		public int RatingUnderweight { get; set; }

		#endregion Public Properties
	}
}
