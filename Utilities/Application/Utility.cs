using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities.Application
{
    public static class Utility
    {
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
	}
}
