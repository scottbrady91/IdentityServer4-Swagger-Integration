using System.Collections.Generic;
using System.Reflection;
using IdentityServer4.AccessTokenValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using NJsonSchema;
using NSwag;
using NSwag.AspNetCore;
using NSwag.SwaggerGeneration.Processors.Security;

namespace Api.NSwag
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            services.AddAuthentication(IdentityServerAuthenticationDefaults.AuthenticationScheme)
                .AddIdentityServerAuthentication(options =>
                {
                    options.Authority = "http://localhost:5000"; // auth server base endpoint (will use to search for disco doc)
                    options.ApiName = "demo_api"; // required audience of access tokens
                    options.RequireHttpsMetadata = false; // dev only!
                });

            services.AddSwagger();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseDeveloperExceptionPage();

            app.UseAuthentication();

            app.UseSwaggerUi(typeof(Startup).GetTypeInfo().Assembly, settings =>
            {
                settings.GeneratorSettings.DefaultPropertyNameHandling = PropertyNameHandling.CamelCase;
                settings.SwaggerUiRoute = "";

                settings.GeneratorSettings.DocumentProcessors.Add(new SecurityDefinitionAppender("oauth2", new SwaggerSecurityScheme
                {
                    Type = SwaggerSecuritySchemeType.OAuth2,
                    Flow = SwaggerOAuth2Flow.Implicit,
                    AuthorizationUrl = "http://localhost:5000/connect/authorize",
                    Scopes = new Dictionary<string, string> {{"demo_api", "Demo API - full access"}}
                }));

                settings.GeneratorSettings.OperationProcessors.Add(new OperationSecurityScopeProcessor("oauth2"));
                
                settings.OAuth2Client = new OAuth2ClientSettings
                {
                    ClientId = "demo_api_swagger",
                    AppName = "Demo API - Swagger"
                };
            });

            app.UseMvc();
        }
    }
}
