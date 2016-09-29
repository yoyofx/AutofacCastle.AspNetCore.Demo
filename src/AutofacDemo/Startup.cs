using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Autofac;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Autofac.Extensions.DependencyInjection;
using Autofac.Extras.DynamicProxy.Core;
using AutofacDemo.Codes;
using Castle.DynamicProxy;

namespace AutofacDemo
{
    public class Startup
    {
        #region Autofac注册
        public IContainer ApplicationContainer { get; private set; }
        
        private IServiceProvider RegisterAutofac(IServiceCollection services)
        {
            var builder = new ContainerBuilder();
            builder.Populate(services);

            var assembly = this.GetType().GetTypeInfo().Assembly;

            builder.RegisterType<AopInterceptor>();


            builder.RegisterAssemblyTypes(assembly)
                  .Where(type =>
                         typeof(IDependency).IsAssignableFrom(type) && !type.GetTypeInfo().IsAbstract)
                  .AsImplementedInterfaces()
                  .InstancePerLifetimeScope()
                  .EnableInterfaceInterceptors().InterceptedBy(typeof(AopInterceptor));


            this.ApplicationContainer = builder.Build();

            return new AutofacServiceProvider(this.ApplicationContainer);
        }

        #endregion


        /*
        * --修改ConfigureServices函数---------------------------------------------------------------------------------------------------------------------------------
        */

        /// <summary>
        /// 这里修改了ConfigureServices函数，加入返回值IServiceProvider，返回Autofac的ServiceProvider
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddApplicationInsightsTelemetry(Configuration);

            services.AddMvc();

            
            return RegisterAutofac(services);
        }


        /*
         * --无变化------------------------------------------------------------------------------------------------------------------------------------------------
         */

        #region 模板生成 

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            if (env.IsDevelopment())
            {
                // This will push telemetry data through Application Insights pipeline faster, allowing you to view results immediately.
                builder.AddApplicationInsightsSettings(developerMode: true);
            }
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }




        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            app.UseApplicationInsightsRequestTelemetry();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseApplicationInsightsExceptionTelemetry();

            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }

        #endregion
    }
}
