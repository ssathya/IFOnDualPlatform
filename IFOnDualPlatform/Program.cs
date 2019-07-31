using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;

namespace IFOnDualPlatform
{
	public class Program
	{


		#region Public Methods

		public static void Main(string[] args)
		{
			CreateWebHostBuilder(args).Build().Run();
		}

		#endregion Public Methods


		#region Private Methods

		private static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
					WebHost.CreateDefaultBuilder(args)
				.ConfigureLogging((hostingContext, logging) =>
				{
					logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
					logging.AddConsole();
					logging.AddDebug();
					logging.AddNLog();
				})
				.UseStartup<Startup>();

		#endregion Private Methods

	}
}