using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Utilities.StringHelpers
{
	public static class StringExtensions
	{
		#region Public Methods

		/// <summary>
		/// Converts/removes all non-ASCII to ASCII.
		/// </summary>
		/// <param name="inString">The in string.</param>
		/// <returns></returns>
		public static string ConvertAllToASCII(string inString)
		{
			var newStringBuilder = new StringBuilder();
			newStringBuilder.Append(inString.Normalize(NormalizationForm.FormKD)
											.Where(x => x < 128)
											.ToArray());
			return newStringBuilder.ToString();
		}

		/// <summary>
		/// Converts string to SSML.
		/// </summary>
		/// <param name="unformatedMsg">The unformated MSG.</param>
		/// <returns></returns>
		public static string ConvertToSSML(string unformatedMsg)
		{
			StringBuilder tempValue = new StringBuilder();
			tempValue.Append("<speak>");
			tempValue.Append(unformatedMsg);
			tempValue.Append("</speak>");
			var retValue = Regex.Replace(tempValue.ToString(), @"\r\n?|\n|\\n|\\r\\n", @"<break strength='x - strong' time='500ms' />");
			return retValue;
		}

		/// <summary>
		/// Determines whether string [is null or white space].
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns>
		///   <c>true</c> if [is null or white space] [the specified value]; otherwise, <c>false</c>.
		/// </returns>
		public static bool IsNullOrWhiteSpace(this string value)
		{
			return string.IsNullOrWhiteSpace(value);
		}

		/// <summary>
		/// Replaces the first.
		/// </summary>
		/// <param name="text">The text.</param>
		/// <param name="search">The search.</param>
		/// <param name="replace">The replace.</param>
		/// <returns></returns>
		public static string ReplaceFirst(this string text, string search, string replace)
		{
			int pos = text.IndexOf(search);
			if (pos < 0)
			{
				return text;
			}
			return text.Substring(0, pos) + replace + text.Substring(pos + search.Length);
		}

		/// <summary>
		/// Converts to Thousands, millions, and billions.
		/// </summary>
		/// <param name="num">The number.</param>
		/// <returns></returns>
		public static string ToKMB(this decimal num)
		{
			if (num > 999999999 || num < -999999999)
			{
				return num.ToString("0,,,.### Billions", CultureInfo.InvariantCulture);
			}
			else
			if (num > 999999 || num < -999999)
			{
				return num.ToString("0,,.## Millions", CultureInfo.InvariantCulture);
			}
			else
			if (num > 999 || num < -999)
			{
				return num.ToString("0,.# Thousands", CultureInfo.InvariantCulture);
			}
			else
			{
				return num.ToString(CultureInfo.InvariantCulture);
			}
		}

		/// <summary>
		/// Truncates a string at word rather than at a number.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <param name="length">The length.</param>
		/// <returns></returns>
		public static string TruncateAtWord(this string value, int length)
		{
			if (value == null || value.Length < length || value.IndexOf(" ", length) == -1)
				return value;

			return value.Substring(0, value.IndexOf(" ", length));
		}

		#endregion Public Methods
	}
}