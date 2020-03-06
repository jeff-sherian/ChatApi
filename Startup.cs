using ChatApi.Helpers;
using ChatApi.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SignalRChat.Hubs;

namespace ChatApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(
                o => o.AddPolicy("CorsPolicy",
                    builder =>
                    {
                        builder.WithOrigins(new string[] { "https://localhost:44308" })
                               .AllowAnyMethod()
                               .AllowAnyHeader()
                               .AllowCredentials();
                    }
                )
            );

            services.AddSignalR(
                options =>
                {
                    options.EnableDetailedErrors = true;
                }
            );
            
            // This would eventually get replaced with built-in JWT Authenticaion
            services.AddSingleton<IAuthHelper, AuthHelper>();

            services.AddHttpContextAccessor();
            services.AddSingleton<IGroupHelper, GroupHelper>();
            services.AddSingleton<IUserHelper, UserHelper>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseCors("CorsPolicy");

            app.UseMiddleware<WebSocketsMiddleware>();
            app.UseMiddleware<ExceptionMiddleware>();

            app.UseAuthentication();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<ChatHub>("/chatHub");
            });
        }
    }
}
