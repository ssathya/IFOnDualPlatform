using System;
using System.Collections.Generic;
using System.Text;

namespace Utilities.Application
{
	public static class Utility
	{

		#region Private Fields

		private static readonly Random rng = new Random();

		#endregion Private Fields


		#region Public Methods

		public static string ConvertToSSML(string unformatedMsg)
		{
			StringBuilder tempValue = new StringBuilder();
			tempValue.Append("<speak>");
			tempValue.Append(unformatedMsg);
			tempValue.Append("</speak>");
			tempValue.Replace("\r", "");
			tempValue.Replace("\n\n", "\n");
			tempValue.Replace("\n", @"<break strength='x - strong' time='500ms' />");
			return tempValue.ToString();
		}

		public static string EndOfCurrentRequest()
		{
			return "\nAnything more? For help say help or say 'bye bye' to quit\n";
		}

		public static string ErrorReturnMsg()
		{
			return "\nWe are experiencing issues obtaining data; be assured we'll resolve as soon as possible\n" +
				EndOfCurrentRequest();
		}

		public static void Shuffle<T>(this IList<T> list)
		{
			int n = list.Count;
			while (n > 1)
			{
				n--;
				int k = rng.Next(n + 1);
				T value = list[k];
				list[k] = list[n];
				list[n] = value;
			}
		}

		#endregion Public Methods
	}
}