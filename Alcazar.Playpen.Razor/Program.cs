using Alcazar.Playpen.Models;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.OData;
using Microsoft.AspNetCore.OData.Formatter.Serialization;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Newtonsoft.Json.Linq;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Web.Http.OData.Formatter.Serialization;

namespace Alcazar.Playpen.Razor
{
	static public class Program
	{
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region Program main method
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		static public void Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);

			// Add template-standard services to the container.
			builder.Services.AddRazorPages();

			// Add additional services to the container.
			builder.Services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();

			// Add MVC controllers
			builder.Services.AddControllers()
							.AddJsonOptions(options =>
							 {
								 // Use PascalCase property names (preserve original model property names)
								 options.JsonSerializerOptions.PropertyNamingPolicy = null;
								 options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
								 options.JsonSerializerOptions.Converters.Add(new DateTimeUsageJsonConverter());
							 });

			//// Configure JSON serialization to not convert DateTime to local time
			//builder.Services.Configure<JsonOptions>(options =>
			//{
			//	options.SerializerOptions.PropertyNamingPolicy = null;
			//});

			// ADD OData controllers
			// builder.AddOData();

			builder.Services.AddSession(options =>
			{
				options.IdleTimeout = TimeSpan.FromMinutes(30);
				options.Cookie.HttpOnly = true;
				options.Cookie.IsEssential = true;
			});

			var app = builder.Build();

			// Configure the HTTP request pipeline.
			if (!app.Environment.IsDevelopment())
			{
				app.UseExceptionHandler("/Error");
				// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
				app.UseHsts();
			}

			app.UseHttpsRedirection();
			app.UseStaticFiles();

			app.UseRouting();
			app.UseSession();

			app.UseAuthorization();

			app.MapRazorPages();

			// Configure MVC routing with default pattern
			app.UseEndpoints(endpoints =>
			{
				// Default routing - always present (done by MapDefaultControllerRoute)
				//endpoints.MapControllerRoute(
				//	name: "default",
				//	pattern: "{controller=Home}/{action=Index}/{id?}");
				endpoints.MapDefaultControllerRoute();
			});

			// Needed for MVC and ODATA
			app.MapControllers();

			app.Run();
		}

		#endregion

		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
		#region Program methods: OData
		//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

		static private void AddOData(this WebApplicationBuilder builder)
		{
			// Configure JSON serialization to not convert DateTime to local time
			builder.Services.Configure<JsonOptions>(options =>
			{
				options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
				// Preserve DateTime Kind and don't convert to local time
				options.SerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
				options.SerializerOptions.Converters.Add(new PreserveDateTimeConverter());
			});

			// Add OData services
			builder.Services.AddControllers().AddOData(options =>
			{
				options.Select().Filter().OrderBy().Expand().Count().SetMaxTop(null)
					.AddRouteComponents("odata", GetEdmModel(), services =>
					{
						// Configure OData to use custom serialization
						services.AddSingleton<IODataSerializerProvider, CustomODataSerializerProvider>();
					});
			}).AddJsonOptions(options =>
			{
				// Configure JSON options for controllers as well
				options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
				options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
				options.JsonSerializerOptions.Converters.Add(new PreserveDateTimeConverter());
			});
		}

		static private Microsoft.OData.Edm.IEdmModel GetEdmModel()
		{
			ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
			builder.EntitySet<DateItem>("Dates");
			return builder.GetEdmModel();
		}

		#endregion
	}
}
