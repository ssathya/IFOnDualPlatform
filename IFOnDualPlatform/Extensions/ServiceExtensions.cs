using Amazon;
using AutoMapper;
using ExternalInterface.BusLogic;
using IFOnDualPlatform.Methods;
using Microsoft.Extensions.DependencyInjection;
using Models.Application;
using MongoHandler;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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
			services.AddScoped<ICommonMethods, CommonMethods>();
			services.AddScoped<IDBConnectionHandler<LastUpateCollectionMd>, DBConnectionHandler<LastUpateCollectionMd>>();
			services.AddScoped<IDBConnectionHandler<PiotroskiScoreMd>, DBConnectionHandler<PiotroskiScoreMd>>();
			services.AddScoped<IDBConnectionHandler<SecuritySymbolMd>, DBConnectionHandler<SecuritySymbolMd>>();
			services.AddScoped<ObtainFundamentals>();
			services.AddScoped<ObtainGoodInvestments>();
			services.AddScoped<ObtainNews>();
			services.AddScoped<ObtainStockQuote>();
			services.AddScoped<ResolveCompanyName>();
		}
		public static void SetupMappings(this IServiceCollection services)
		{
			var config = new MapperConfiguration(cfg =>
			{
				cfg.CreateMap<PiotroskiScore, PiotroskiScoreMd>()
				.ForMember(d => d.Id, t => t.Ignore())
					.ReverseMap();
				cfg.CreateMap<SecuritySymbol, SecuritySymbolMd>()
				.ForMember(d => d.Id, t => t.Ignore())
					.ReverseMap();
			});
			var mapper = config.CreateMapper();
			services.AddSingleton(mapper);
		}
	}
}
