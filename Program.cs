
using System.Text;
using BookAPI.Data;
using BookAPI.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace BookAPI
{
    public class Program
    {
	    public Program(IConfiguration configuration)
	    {
		    Configuration = configuration;
	    }
	    public IConfiguration Configuration { get; }

		public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();

            builder.Services.AddDbContext<ApplicationDbContext>(e =>
	            e.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddIdentity<User, IdentityRole>()
	            .AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();

            // Authorize action on swagger
            builder.Services.AddSwaggerGen(c =>
            {
	            c.SwaggerDoc("v1", new OpenApiInfo { Title = "ToDo API", Version = "v1" });

	            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
	            {
		            Name = "Authorization",
		            Type = SecuritySchemeType.ApiKey,
		            Scheme = "Bearer",
		            BearerFormat = "JWT",
		            In = ParameterLocation.Header,
		            Description = "Enter 'Bearer' followed by your JWT token.\nExample: Bearer eyJhbGciOiJIUzI1NiIs..."
	            });

	            c.AddSecurityRequirement(new OpenApiSecurityRequirement
	            {
		            {
			            new OpenApiSecurityScheme
			            {
				            Reference = new OpenApiReference
				            {
					            Type = ReferenceType.SecurityScheme,
					            Id = "Bearer"
				            }
			            },
			            new string[] { }
		            }
	            });
            });

            //Validating token
            builder.Services.AddAuthentication(options =>
	            {
		            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
		            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
		            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
	            })
	            .AddJwtBearer(options =>
		            {
			            options.SaveToken = true;
			            options.RequireHttpsMetadata = false;
			            options.TokenValidationParameters = new TokenValidationParameters()
			            {
				            ValidateIssuer = true,
				            ValidateAudience = true,
				            ValidateLifetime = true,
				            ValidateIssuerSigningKey = true,
				            ValidIssuer = builder.Configuration["JWT:Issuer"],
				            ValidAudience = builder.Configuration["JWT:Audience"],
				            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:Secret"]))
			            };
		            }
	            );

            builder.Services.AddAuthorization();
            builder.Services.AddControllers(); 
            
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthentication();
			app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
