using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Alexa.NET.Request;
using Alexa.NET.Response;
using Google.Cloud.Dialogflow.V2;
using Microsoft.AspNetCore.Http;
using Models.Reqeusts;
using RestSharp;

namespace IFOnDualPlatform.Methods
{
	public interface ICommonMethods
	{
		void ProcessIntends(WebhookRequest value, string intentName, ref IAppRequest iRequest, ref string controllerName);
		void SetupAPICall(IAppRequest iRequest, string controllerName, out RestClient clinet, out RestRequest request, HttpRequest webRequest);
		void ProcessIntends(SkillRequest skillRequest, ref SkillResponse skillResponse, string intentName, ref IAppRequest iRequest, ref string controllerName);
	}
}
