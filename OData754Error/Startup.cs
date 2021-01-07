using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Formatter.Serialization;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.OData;
using Microsoft.OData.Edm;

using Newtonsoft.Json;

namespace OData_754_Error
{
    public class Startup
    {

        private readonly ILogger _logger;

        public Startup(IConfiguration configuration, ILogger<Startup> logger)
        {
            Configuration = configuration;
            _logger = logger;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public virtual void ConfigureServices(IServiceCollection services)
        {
            services.AddOData();

            services.AddMvc(x =>
            {
                x.EnableEndpointRouting = false;

            })
            .SetCompatibilityVersion(CompatibilityVersion.Version_2_1)
            .AddApplicationPart(Assembly.GetExecutingAssembly());

            //services.AddEntityFrameworkInMemoryDatabase();
            services.AddDbContext<EFContext>(x => x.UseInMemoryDatabase("Test"));
            
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public virtual void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, IApplicationLifetime applicationLifetime)
        {
            var logger = loggerFactory.CreateLogger(typeof(Startup));

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc(routeBuilder =>
            {
                routeBuilder.MapRoute("default", "{controller=Home}/{action=Index}");

                routeBuilder.EnableDependencyInjection();

                //Enables odata query options (for all entities)
                routeBuilder.Select().Expand().Filter().OrderBy().MaxTop(int.MaxValue).SkipToken().Count();

                Func<IServiceProvider, IEdmModel> modelFact = (sp) => CreateVismaCompanyIEdmModel(app.ApplicationServices);

                routeBuilder.MapODataServiceRoute("myroute", "odata", a =>
                {

                    a.AddService(Microsoft.OData.ServiceLifetime.Singleton, typeof(IEdmModel), modelFact)
                        .AddService<ILogger>(Microsoft.OData.ServiceLifetime.Singleton, x => logger)
                        .AddService<ODataSerializerProvider, IgnoreDefaultEntityPropertiesSerializerProvider>(Microsoft.OData.ServiceLifetime.Singleton)
                        .AddService<IEnumerable<IODataRoutingConvention>>(Microsoft.OData.ServiceLifetime.Singleton, sp =>
                              ODataRoutingConventions.CreateDefaultWithAttributeRouting("myroute", routeBuilder));
                });



            });
        }

        private IEdmModel CreateVismaCompanyIEdmModel(IServiceProvider applicationServices)
        {
            ODataConventionModelBuilder odataBuilder = new ODataConventionModelBuilder();
            using (var scope = applicationServices.CreateScope())
            {
                var ctx = scope.ServiceProvider.GetRequiredService<EFContext>();

                for (int i = 1; i < 1000; i++)
                {
                    ctx.Add(new ODataEntity
                    {
                        Id = i,
                        SomeOtherProperty = Guid.NewGuid().ToString(),
                    });
                }

                ctx.SaveChanges();

                foreach (var efType in ctx.Model.GetEntityTypes())
                {
                    //Odata entity configuration for the entity framework entity
                    var odataEntityTypeConfig = odataBuilder.AddEntityType(efType.ClrType);

                    //Map the EF primary keys to Odata model
                    foreach (var efKey in efType.FindPrimaryKey().Properties)
                    {
                        odataEntityTypeConfig.HasKey(efKey.PropertyInfo);
                    }


                    //Handle field specific edm configuration
                    foreach (var efProperty in efType.GetProperties().Where(x => !x.IsShadowProperty()))
                    {
                        var edmPropertyConfig = odataEntityTypeConfig.AddProperty(efProperty.PropertyInfo);
                    }

                    //Add odata entity configuration to odata model
                    odataBuilder.AddEntitySet(efType.ClrType.Name, odataEntityTypeConfig);
                }
            }


            return odataBuilder.GetEdmModel();
        }
    }
}
