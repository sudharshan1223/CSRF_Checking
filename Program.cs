using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption.ConfigurationModel;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption;
using Microsoft.AspNetCore.Builder;
using Microsoft.JSInterop;
using System.Diagnostics;


namespace WebApplication1
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowSpecificOrigin",
                    builder =>
                    {
                        builder.WithOrigins("https://yourblazorappdomain.com")
                               .AllowAnyHeader()
                               .AllowAnyMethod();
                    });
            });
            builder.Services.AddControllers();
            builder.Services.AddHttpContextAccessor();

            builder.Services.AddAntiforgery(options =>
            {
                // Configure anti-forgery settings
                options.Cookie.Name = "X-XSRF-TOKEN";
                options.SuppressXFrameOptionsHeader = true;
                //options.HeaderName = "RequestVerificationToken";
            });
            builder.Services.AddDataProtection()
                     .PersistKeysToFileSystem(new DirectoryInfo(@"C:\Users\iw561f\Desktop"))
                     .SetApplicationName("WebApplication123")
                     .UseCryptographicAlgorithms(new AuthenticatedEncryptorConfiguration
                     {
                         EncryptionAlgorithm = EncryptionAlgorithm.AES_256_CBC,
                         ValidationAlgorithm = ValidationAlgorithm.HMACSHA256
                     })
                     .SetDefaultKeyLifetime(TimeSpan.FromDays(180));
            builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme)
                .AddNegotiate();
            builder.Services.AddAuthorization(options =>
            {
                // By default, all incoming requests will be authorized according to the default policy.
                options.FallbackPolicy = options.DefaultPolicy;
            });
            var app = builder.Build();
            app.UseCors("AllowSpecificOrigin");

            app.UseAntiforgery();
            app.UseHttpsRedirection();

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }

}
