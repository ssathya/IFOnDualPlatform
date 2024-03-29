﻿namespace Models.Application
{
	public class DetectResult
	{
		public string Language { get; set; }
		public float Score { get; set; }
		public bool IsTranslationSupported { get; set; }
		public bool IsTransliterationSupported { get; set; }
		public AltTranslations[] Alternatives { get; set; }
	}
}