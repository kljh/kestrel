using Microsoft.AspNetCore.Authentication.Certificate;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public static class Program
    {
        internal static async Task<int> Main(string[] args)
        {            
            var builder = CreateHostBuilder();
            var host = builder.Build();
            b host.StartAsync();
            await host.WaitForShutdownAsync();
            return 0;
        }

        public static IHostBuilder CreateHostBuilder() => 
            Host.CreateDefaultBuilder()
            .ConfigureWebHost((webBuilder) => {
                webBuilder
                .UseKestrel((context, options) => {
                    options.ConfigureHttpsDefaults(o => {
                        // o.ServerCertificate = cert;
                        o.ClientCertificateMode = ClientCertificateMode.AllowCertificate;  // Allow or Require
                    });

                    options.ListenAnyIP(8086);
                    options.ListenAnyIP(8064, lo => {
                        lo.UseHttps();
                    });
                })
                .UseStartup<StartUp>();
            });
    }

    public class StartUp
    {
        public void ConfigureServices(IServiceCollection services) {
            services.AddRouting();

            services
            .AddAuthentication(CertificateAuthenticationDefaults.AuthenticationScheme)
            .AddCertificate(options => {
                options.AllowedCertificateTypes = CertificateTypes.All;  // Chain, Self-Signe, or Both
                // options.RevocationFlag = X509RevocationFlag.ExclureRoot;
                // options.Events = ...
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseRouting();
            // app.UseCors();
            app.UseStaticFiles(new StaticFileOptions {
                RequestPath = "/static",
                FileProvider = new PhysicalFileProvider(Path.Combine(env.ContentRootPath, "static"))
            });

            
            app.Use((context, next) => {
                return next(context);
            });
            
            
            app.UseEndpoints(endpoints => {
                endpoints.MapGet("/", async (context) => {
                    X509Certificate2? cert = await context.Request.HttpContext.Connection.GetClientCertificateAsync();
                    bool isVerified = cert==null ? false : cert.Verify();
                    string id = (cert==null || string.IsNullOrEmpty(cert.Subject)) ? "bel inconnu" : cert.Subject;

                    context.Response.ContentType = "text/html";
                    // context.Response.StatusCode = 200;

                    var msg = $"Bonjour {id}. Https = {context.Request.IsHttps}, Cert verified = {isVerified}.";
                    await context.Response.Body.WriteAsync(ASCIIEncoding.ASCII.GetBytes(msg));
                });
            });
        }
    }

}