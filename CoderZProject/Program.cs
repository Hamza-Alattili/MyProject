using Infrastructure.Context;
using Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using Application.Services.Interfaces;


var builder = WebApplication.CreateBuilder(args);
builder.Services.AddScoped<IAuthService, Application.Services.AuthService>();
builder.Services.AddScoped<IDoctorService, Application.Services.DoctorService>();
builder.Services.AddScoped<IAppointmentService, Application.Services.AppointmentService>();

// Add services to the container.

builder.Services.AddDbContext<ProjectDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default"))
    );


builder.Services.AddHttpContextAccessor();


var jwtSection = builder.Configuration.GetSection("Jwt");


builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSection["Issuer"],
            ValidAudience = jwtSection["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSection["Key"])),

        };
    });



var app = builder.Build();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy =>
        {
            policy
                .WithOrigins("*")
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});





builder.Services.AddControllers();
builder.Services.AddAuthorization();




app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
using(var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<Infrastructure.Context.ProjectDbContext>();
    await Infrastructure.Data.UserSeedData.InitializeAsync(context);
}

app.Run();
