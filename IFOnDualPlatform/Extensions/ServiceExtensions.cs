using Amazon;
using ExternalInterface.BusLogic;
using IFOnDualPlatform.Methods;
using Microsoft.Extensions.DependencyInjection;
using Models.Application;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities.Amazon.S3;
using Utilities.Application;

namespace IFOnDualPlatform.Extensions
{
    public static class ServiceExtensions
    {
		public static string BucketName = @"talk2control-1";
		public static RegionEndpoint Region = RegionEndpoint.USEast1;

		public static void AddKeysToEnvironment(this IServiceCollection services)
		{
			var readS3Objs = new ReadS3Objects(BucketName, Region);
			var keysToServices = JsonConvert
				.DeserializeObject<List<EntityKeys>>(readS3Objs
					.GetEncryptedDataFromS3("Random.txt")
				.Result);
			foreach (var entityKeys in keysToServices)
			{
				if (!string.IsNullOrEmpty(entityKeys.Entity)
					&& !string.IsNullOrEmpty(entityKeys.Key))
					Environment.SetEnvironmentVariable(entityKeys.Entity, entityKeys.Key);
			}
		}
		public static void SetupDependencies(this IServiceCollection services)
		{
			services.AddScoped<EnvHandler>();
			services.AddScoped<ObtainNews>();
			services.AddScoped<ICommonMethods, CommonMethods>();
		}
	}
}
