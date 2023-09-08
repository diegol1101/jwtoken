
using Microsoft.AspNetCore.Identity;
using Dominio.Entities;
using Dominio.Interfaces;
using Aplicacion.UnitOfWork;
using Microsoft.AspNetCore.Authorization;
using API.Services;
using API.Helpers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace API.Extensions;

    public static class ApplicationServiceExtension
    {
        public static void ConfigureCors(this IServiceCollection services) =>
        services.AddCors(options=>
        {
            options.AddPolicy("CorsPolicy", builder =>
            builder.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
        });
        public static void AddAplicacionServices(this IServiceCollection services)
            {
                //Services.AddScoped<IpaisInterface,PaisRepository>();
                //Services.AddScoped<ITipoPersona,TipoPeronsaRepository>();
                
                services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
                services.AddScoped<IUnitOfWork, UnitOfWork>();
                services.AddScoped<IUserService, UserService>();
                services.AddScoped<IAuthorizationHandler, GlobalVerbRoleHandler>();
            }

        public static void AddJwt(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<JWT>(configuration.GetSection("JWT"));

            services.AddAuthentication(options=>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
                .AddJwtBearer(o=>
                {
                    o.RequireHttpsMetadata= false;
                    o.SaveToken = false;
                    o.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey=true,
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero,
                        ValidIssuer = configuration["JWT:Issuer"],
                        ValidAudience = configuration["JWT:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT:Key"]))



                    };
                });
        }
    }
