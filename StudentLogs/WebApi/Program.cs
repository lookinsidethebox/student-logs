using Core.Abstractions;
using Core.EF;
using Core.Entities;
using Core.Helpers;
using Core.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using IdentityOptions = Core.Options.IdentityOptions;
using PasswordOptions = Core.Models.PasswordOptions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = IdentityOptions.ISSUER,
        ValidateAudience = true,
        ValidAudience = IdentityOptions.AUDIENCE,
        ValidateLifetime = true,
        IssuerSigningKey = IdentityOptions.GetSymmetricSecurityKey(),
        ValidateIssuerSigningKey = true
    };
});

builder.Services.AddAuthorization();

builder.Services.AddDbContext<DataContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.Configure<PasswordOptions>(builder.Configuration.GetSection(nameof(PasswordOptions)));

builder.Services.AddSingleton(typeof(IRepository<>), typeof(BaseRepository<>));
builder.Services.AddSingleton<IAuthService, AuthService>();
builder.Services.AddSingleton<IPasswordService, PasswordService>();
builder.Services.AddSingleton<IDataContextOptionsHelper, DataContextOptionsHelper>();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers().RequireAuthorization();

app.Run();
