using Core.Options;
using Core.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

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
//builder.Services.AddAuthorization();

builder.Services.AddSingleton<IAuthService, AuthService>();

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
