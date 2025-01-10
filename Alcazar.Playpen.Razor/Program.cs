using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace Alcazar.Playpen.Razor
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);

			// Add template-standard services to the container.
			builder.Services.AddRazorPages();

			// Add additional services to the container.
			builder.Services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();

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

			app.UseAuthorization();

			app.MapRazorPages();

			app.Run();
		}
	}
}
