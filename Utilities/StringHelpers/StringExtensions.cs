using Models.Application;
using Newtonsoft.Json;
using System;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using Utilities.Application;

namespace Utilities.StringHelpers
{
	public static class StringExtensions
	{

		#region Private Fields

		private const string HostName = "https://api.cognitive.microsofttranslator.com";
		private const string KeyForTraslator = "ssTranslatorService0801";
		private const string RouteName = "/detect?api-version=3.0";

		#endregion Private Fields

		#region Public Methods

		/// <summary>
		/// Converts/removes all non-ASCII to ASCII.
		/// </summary>
		/// <param name="inString">The in string.</param>
		/// <returns></returns>
		public static string ConvertAllToASCII(this string inString)
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
		/// <param name="unformattedMsg">The unformatted MSG.</param>
		/// <returns></returns>
		public static string ConvertToSSML(this string unformatted)
		{

			StringBuilder tempValue = new StringBuilder();
			unformatted = unformatted.Replace("&", " and ")
				.Replace(">", " greater than ")
				.Replace("<", " less than ")
				.Replace("'", "")
				.Replace("\"", "");
			tempValue.Append("<speak>");
			tempValue.Append(unformatted);
			tempValue.Append("</speak>");
			var retValue = Regex.Replace(tempValue.ToString(), @"\r\n?|\n|\\n|\\r\\n", @"<break time='250ms'/>");
			//remove too may breaks
			string pattern = @"<break time='250ms'\/>\s*<break time='250ms'\/>";
			string substitution = @"<break time='250ms'/>";
			RegexOptions options = RegexOptions.Multiline;
			Regex regex = new Regex(pattern, options);
			for (int i = 0; i < 3; i++)
			{				
				retValue = regex.Replace(retValue, substitution);
			}

			return retValue;
		}

		public static bool IsEnglish(this string inputString)
		{
			var envHandler = new EnvHandler();
			var key = envHandler.GetApiKey(KeyForTraslator);
			string host = HostName;
			string route = RouteName;
			return DetectTextRequest(key, host, route, inputString.TruncateAtWord(50));
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

		public static string StripSpecialChar(this string text)
		{
			string pattern = @"(\""|\.|\?|\$\!)";
			string substitution = @"";
			RegexOptions options = RegexOptions.Multiline;
			Regex regex = new Regex(pattern, options);
			var stripped = regex.Replace(text, substitution);
			return stripped;
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


		#region Private Methods

		private static bool DetectTextRequest(string subscriptionKey, string host, string route, string inputText)
		{
			object[] body = new object[] { new { Text = inputText } };
			var requestBody = JsonConvert.SerializeObject(body);

			using (var client = new HttpClient())
			using (var request = new HttpRequestMessage())
			{
				// Build the request.
				request.Method = HttpMethod.Post;
				request.RequestUri = new Uri(host + route);
				request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");
				request.Headers.Add("Ocp-Apim-Subscription-Key", subscriptionKey);

				// Send the request and get response.
				//HttpResponseMessage response = await client.SendAsync(request).ConfigureAwait(false);
				var response = client.SendAsync(request).Result;
				// Read response as a string.
				string result = response.Content.ReadAsStringAsync().Result;
				DetectResult[] deserializedOutput = JsonConvert.DeserializeObject<DetectResult[]>(result);
				if (deserializedOutput.Any() == false)
				{
					return false;
				}
				return deserializedOutput[0].Language == "en";
			}
		}

		#endregion Private Methods
	}
}