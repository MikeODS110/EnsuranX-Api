using Ensuranx.Application.Contracts;
using Ensuranx.Application.Interfaces;
using Ensuranx.Domain.IdentityExtensions;
using Ensuranx.Infrastrcuture.Shared.Configuration;
using Ensuranx.Infrastructure.DbContext;
using Ensuranx.Infrastructure.Repositories;
using Ensuranx.Api.Common.Errors;
using Ensuranx.Application.Contracts;
using Ensuranx.Application.Interfaces;
using Ensuranx.Domain.IdentityExtensions;
using Ensuranx.Infrastrcuture.Shared.Configuration;
using Ensuranx.Infrastrcuture.Shared.Services;
using Ensuranx.Infrastructure.DbContext;
using Ensuranx.Infrastructure.Repositories;
using Ensuranx.Infrastructure.Services.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;
using static Ensuranx.Domain.Enums.Enum;
using static Ensuranx.Domain.Enums.Enum;
using Ensuranx.Application.Contracts.Providers;
using Ensuranx.Infrastructure.Services.Provider;

var builder = WebApplication.CreateBuilder(args);
{
    // Add services to the container.
    builder.Services.AddControllers();
    builder.Services.AddSingleton<ProblemDetailsFactory, EnsuranxProblemDetailsFactory>();
    // Set up Connection String with DB Context
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

    builder.Services.AddDefaultIdentity<AppUser>(options => {
        options.SignIn.RequireConfirmedAccount = false;
        options.User.RequireUniqueEmail = true;
        options.Password.RequireDigit = false;
        options.Password.RequiredLength = 6;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireLowercase = false;
    }).AddRoles<AppRole>().AddEntityFrameworkStores<ApplicationDbContext>();

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("CorsPolicy",
            builder => builder.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
    });
    builder.Services.AddHttpClient();


    //adding jwt authentication
    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    }).AddJwtBearer(o =>
    {
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = false,
            ValidateIssuerSigningKey = true
        };
    });

    builder.Services.AddAuthorization();
    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
    builder.Services.Configure<MailSettings>(builder.Configuration.GetSection(nameof(MailSettings)));
    builder.Services.AddTransient<IEmailService, MailService>();
    builder.Services.AddTransient<IUserInfoRepository, UserInfoRepository>();
    builder.Services.AddTransient<IRoleRepository, RoleRepository>();
    //builder.AddScoped<IProviderService, ProviderService>();


    builder.Host.UseSerilog((context, configuration) =>
        configuration.ReadFrom.Configuration(context.Configuration));

    //builder.Services.AddTransient<IUserService, UserService>();
    //var logger = new LoggerConfiguration()
    //    .WriteTo.Map("controller", "(controller)", (ctrl, wt) => wt.File($"C:\\Logs\\{ctrl}\\lis_api.log")).Enrich.FromLogContext().CreateLogger();
    //builder.Host.UseSerilog(logger);
}

var app = builder.Build();

{
    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }
    app.UseCors("CorsPolicy");
    app.MapControllerRoute(
       name: "Admin",
       pattern: "{area:exists}/{controller=APIHome}/{action=Index}/{id?}");
    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=APIHome}/{action=Index}/{id?}");

    app.UseExceptionHandler("/error");
    app.UseHttpsRedirection();
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();
    app.UseSerilogRequestLogging();
    //CreateRoles(app);
 

    void CreateRoles(WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        using var RoleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();

        string[] roleNames = { Roles.Admin.ToString(), Roles.User.ToString(), Roles.Guest.ToString()};
        IdentityResult roleResult;
        foreach (var roleName in roleNames)
        {
            var roleExist = RoleManager.RoleExistsAsync(roleName).Result;
            if (!roleExist)
            {
                roleResult = RoleManager.CreateAsync(new IdentityRole<int>(roleName)).Result;
            }
        }
    }

    app.Run();
}
