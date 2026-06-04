using Identity_Auth_MicroService.Domain.Contracts;
using Identity_Auth_MicroService.Domain.Entities.IdenetityModule;
using Identity_Auth_MicroService.Presistance.Data.DataSeed;
using Identity_Auth_MicroService.Presistance.Data.DbContexts;
using Identity_Auth_MicroService.Presistance.Repositories;
using Identity_Auth_MicroService.Services.Services;
using Identity_Auth_MicroService.Servives_Abstraction.Interfaces;
using Identity_Auth_MicroService.Web.CustomMiddleWares;
using Identity_Auth_MicroService.Web.Extentions;
using Identity_Auth_MicroService.Web.Factories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using MyAuthService = Identity_Auth_MicroService.Services.Services.AuthenticationService;
using MyIAuthService = Identity_Auth_MicroService.Services_Abstraction.Interfaces.IAuthenticationService;

namespace Identity_Auth_MicroService.Web
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            #region Add Services to the container
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddDbContext<ClinicIdentityDbContext>(options =>
            {
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString("DefaultConnection"),
                    sqlOptions =>
                    {
                        sqlOptions.EnableRetryOnFailure(
                            maxRetryCount: 5,
                            maxRetryDelay: TimeSpan.FromSeconds(10),
                            errorNumbersToAdd: null);
                    });
            });

            builder.Services.Configure<ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory = ApiResponseFactory.GenerateApiValidationResponse;
            });

            builder.Services.AddScoped<MyIAuthService, MyAuthService>();
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

            builder.Services.AddHttpClient<IClinicClient, ClinicClient>(client =>
            {
                client.BaseAddress = new Uri(builder.Configuration["Clinic:BaseUrl"]!);
            });

            builder.Services.AddIdentityCore<ApplicationUser>(options =>
            {
                // optional
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ClinicIdentityDbContext>()
            .AddSignInManager()
            .AddDefaultTokenProviders();

            // ✅ JWT Authentication (Issuer/Audience/SecretKey)

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.RequireHttpsMetadata = false;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,

                    ValidIssuer = builder.Configuration["JWTOptions:Issuer"],
                    ValidAudience = builder.Configuration["JWTOptions:Audience"],

                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWTOptions:SecretKey"]!)),
                    ClockSkew = TimeSpan.Zero,

                    RoleClaimType = ClaimTypes.Role,
                    NameClaimType = ClaimTypes.NameIdentifier
                };
            });

            // ✅ Authorization Policies
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("AdminOnly", p => p.RequireRole("Admin"));
                options.AddPolicy("DoctorOnly", p => p.RequireRole("Doctor"));
                options.AddPolicy("LabOnly", p => p.RequireRole("Lab"));
                options.AddPolicy("PatientOnly", p => p.RequireRole("Patient"));
            });

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
            });

            builder.Services.AddScoped<IDataIntializer, IdentityDataIntializer>();

            builder.WebHost.ConfigureKestrel(options =>
            {
                options.ListenAnyIP(7179);
            });

            #endregion

            var app = builder.Build();

            #region DataSeed - Apply Migration
            await app.MigrateIdentityDatabaseAsync();
            await app.SeedIdentityDataAsync();
            #endregion

            #region Configure the HTTP request pipeline

            app.UseMiddleware<ExceptionHandlerMiddleWare>();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseRouting();
            app.UseCors("AllowAll");

            app.Use(async (ctx, next) =>
            {
                var path = ctx.Request.Path.Value ?? "";

                // نخليه يشتغل بس على Internal endpoints
                if (path.StartsWith("/Clinic/Authentication/Internal", StringComparison.OrdinalIgnoreCase))
                {
                    var key = ctx.Request.Headers["X-Internal-Key"].FirstOrDefault();

                    if (key != builder.Configuration["Internal:Key"])
                    {
                        ctx.Response.StatusCode = 401;
                        await ctx.Response.WriteAsync("Unauthorized - Invalid Internal Key");
                        return;
                    }
                }

                await next();
            });

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            #endregion

            await app.RunAsync();
        }
    }
}