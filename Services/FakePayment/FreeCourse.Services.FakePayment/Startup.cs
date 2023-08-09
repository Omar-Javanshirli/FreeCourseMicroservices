using Automatonymous.Graphing;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;

namespace FreeCourse.Services.FakePayment
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

            services.AddMassTransit(x =>
            {
                //RabbitMq ucun port =>  Default Port : 5672
                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(Configuration["RabbitMQUrl"], "/", host =>
                    {
                        host.Username("guest");
                        host.Password("guest");
                    });
                });
            });

            services.AddMassTransitHostedService();

            //AuthorizationPolicyBuilder, yetkilendirme politikalarını oluşturmak için kullanılan bir yardımcı sınıftır.
            //Yukarıdaki kodda, RequireAuthenticatedUser metodu ile sadece kimlik doğrulanmış kullanıcıların erişimine izin veren
            //bir politika oluşturuluyor. Bu, sayfaların veya yönlendirmelerin yalnızca oturum açmış kullanıcılar tarafından erişilebileceği anlamına gelir.
            var requireAuthorizePolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();

            //Bu kod, JWT (JSON Web Token) ile ilgili bir ayarı düzenler. JSON Web Token'lar, kullanıcı hakkındaki
            //verileri içerir ve bunlar "claims" olarak adlandırılır. sub ("subject") öneki, JWT içinde kullanıcının benzersiz
            //tanımlayıcısını temsil eder.
            //JwtSecurityTokenHandler.DefaultInboundClaimTypeMap özellikle gelen taleplerdeki JWT'deki "claim" türlerini dönüştürmeye
            //yönelik haritalamayı tutar. Örneğin, "sub" öneki olan bir "claim" gelecekte "
            //http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier" gibi bir türe dönüşebilir.
            //Ancak bu kod parçası, "sub" önekinin hangi "claim" türüne dönüştürülmeyeceğini belirtiyor.
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Remove("sub");

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
            {
                options.Authority = Configuration["IdentityServerURL"];
                options.Audience = "resource_payment";
                options.RequireHttpsMetadata = false;
            });

            services.AddControllers(opt =>
            {
                opt.Filters.Add(new AuthorizeFilter(requireAuthorizePolicy));
            });
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "FreeCourse.Services.FakePayment", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "FreeCourse.Services.FakePayment v1"));
            }

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}