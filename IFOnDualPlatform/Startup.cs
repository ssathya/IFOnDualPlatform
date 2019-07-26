using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace IFOnDualPlatform
{
	public class Startup
	{

		#region Public Properties

		public IConfiguration Configuration { get; }

		#endregion Public Properties


		#region Public Constructors

		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		#endregion Public Constructors

		#region Public Methods

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IHostingEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}
			else
			{
				// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
				app.UseHsts();
			}
			app.UseSwagger();
			app.UseSwaggerUI(c =>
			{
				c.SwaggerEndpoint("/swagger/V3/swagger.json", "Index Flux");
			});
			app.UseHttpsRedirection();
			app.UseMvc();
		}

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

			services.AddSwaggerGen(c =>
			{
				c.SwaggerDoc("V3", new OpenApiInfo { Title = "Index flux", Version = "V3" });
			});
		}

		#endregion Public Methods
	}
}