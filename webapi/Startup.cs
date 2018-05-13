using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using webapi.Models;
using MySql.Data.MySqlClient;
using System.IO;
using Microsoft.Extensions.FileProviders;
using System.Net.WebSockets;

//this both need to be added so UseMySQL works
using MySql.Data.EntityFrameworkCore.Extensions;
using Microsoft.EntityFrameworkCore;
namespace webapi
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
      string connectionString = "server=localhost;port=3306;database=asp;uid=developer;password=developer123;Encrypt=False";
      
      services.AddDbContext<TestDataContext>(options => options.UseMySQL(connectionString));
      services.AddMvc().AddSessionStateTempDataProvider();
      services.AddSession(options =>
      {
        options.Cookie.Name = "GimmeACookieee!";
      });

    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IHostingEnvironment env)
    {
      if (env.IsDevelopment())
      {
        app.UseDeveloperExceptionPage();
      }
      /*app.Use(async (ContextBoundObject, next) =>{
         Console.WriteLine("called middleware :D");
         await next();
      });*/
      app.UseWebSockets();
      app.Use(async (context, next) =>{
        Console.WriteLine("is ws{0}",context.WebSockets.IsWebSocketRequest);
        if (context.Request.Path == "/ws")
        {
          if (context.WebSockets.IsWebSocketRequest)
          {
            WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
            await WebSocketEcho.Echo(context, webSocket);
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
      app.UseSession();
      app.UseStaticFiles(new StaticFileOptions
      {
        FileProvider = new PhysicalFileProvider(
              Path.Combine(Directory.GetCurrentDirectory(), "MyStaticFiles")),
        RequestPath = "/StaticFiles"
      });
      app.UseMvc();
      app.UseStaticFiles(); // For the wwwroot folder
      
    }
  }
}
