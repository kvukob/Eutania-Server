using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Server.Database;
using Server.Hubs;
using Server.Services.Effex;
using Server.Services.Satellite;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var _myAllowSpecificOrigins = "_myAllowSpecificOrigins";

builder.Host.ConfigureServices(services =>
{
    services.AddControllersWithViews();

    services.AddDbContext<GameDbContext>(options =>
        options.UseSqlServer(connectionString)
    );


    services.AddCors(options =>
    {
        options.AddPolicy(_myAllowSpecificOrigins,
            builder =>
            {
                builder.WithOrigins("http://localhost:8080").AllowAnyHeader().AllowAnyMethod().AllowCredentials();
                //builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
            });
    });


    services.AddSignalR();

    services.AddAuthorization();

    services.AddAuthentication(auth =>
    {
        auth.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        auth.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    }).AddJwtBearer(options =>
    {
        // jwt addition
        // get key from settings
        var appSettings = builder.Configuration.GetSection("AppSettings").GetValue<string>("Secret");
        var key = Encoding.ASCII.GetBytes(appSettings);

        //options.Authority = "Authority URL"; // TODO: Update URL
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = false,
            ValidateAudience = false,
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];

                // If the request is for our hub...
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) &&
                    (path.StartsWithSegments("/gamehub")))
                {
                    // Read the token out of the query string
                    context.Token = accessToken;
                }

                return Task.CompletedTask;
            }
        };
    });

    services.AddSpaStaticFiles(configuration: options => { options.RootPath = "wwwroot"; });

    services.AddTransient<IHubSatellite, HubSatellite>();
    services.AddSingleton<IEffex, Effex>();
});
var app = builder.Build();


app.UseCors(_myAllowSpecificOrigins);

app.UseDefaultFiles();
app.UseStaticFiles();
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseRouting();
app.UseAuthorization();

app.UseSpaStaticFiles();
app.UseSpa(configuration: builder => { builder.Options.SourcePath = "wwwroot"; });

app.MapControllers();

app.MapHub<GameHub>("/gamehub");
app.Run();