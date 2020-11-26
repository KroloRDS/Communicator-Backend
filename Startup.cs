using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Net.WebSockets;

using Communicator.WebSockets;
using Communicator.Services;

namespace Communicator
{
	public class Startup
	{
		private IWebSocketHandler _webSocketHandler;
		private readonly string _dbConnectionString;

		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
			_dbConnectionString = 
				Configuration.GetConnectionString("ConnectionParams") +
				System.Environment.CurrentDirectory +
				Configuration.GetConnectionString("DbFileLocation");
		}

		public IConfiguration Configuration { get; }

		public void ConfigureServices(IServiceCollection services)
		{
			services.AddDistributedMemoryCache();

			services.AddSession(options =>
			{
				options.Cookie.HttpOnly = true;
				options.Cookie.IsEssential = true;
			});

			services.AddControllers();
			services.AddScoped<IUserService, UserServiceImpl>();
			services.AddScoped<IMessageService, MessageSerciveImpl>();
			services.AddScoped<IFriendRelationService, FriendRelationServiceImpl>();
			services.AddScoped<IWebSocketHandler, WebSocketHandlerImpl>();
			services.AddDbContext<CommunicatorDbContex>(opt => opt.UseSqlServer(_dbConnectionString));
			services.AddSwaggerGen(c =>
			{
				c.SwaggerDoc("v1", new OpenApiInfo { Title = "Communicator", Version = "v1" });
			});
		}

		public void Configure(
			IApplicationBuilder app,
			IWebHostEnvironment env,
			IWebSocketHandler webSocketHandler,
			IServiceScopeFactory serviceScopeFactory
			)
		{
			_webSocketHandler = webSocketHandler;

			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
				app.UseSwagger();
				app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Communicator v1"));
			}

			app.UseHttpsRedirection();

			app.UseRouting();

			app.UseAuthorization();

			app.UseSession();

			app.UseWebSockets();
			app.Use(async (context, next) =>
			{
				if (context.Request.Path == "/ws")
				{
					if (context.WebSockets.IsWebSocketRequest)
					{
						using var scope = serviceScopeFactory.CreateScope();
						var db = scope.ServiceProvider.GetService<CommunicatorDbContex>();
						WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
						await _webSocketHandler.Handle(webSocket, db);
					}
					else
					{
						context.Response.StatusCode = 400;
					}
				}
				else
				{
					await next();
				}
			});

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllers();
			});
		}
	}
}
