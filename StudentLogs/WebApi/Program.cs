using Core;
using Core.Abstractions;
using Core.EF;
using Core.Helpers;
using Core.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
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

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(
        policy =>
        {
            policy
                .WithOrigins("http://sequencing-project.ru")
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});

builder.Services.AddAuthorization();

builder.Services.AddDbContext<DataContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.Configure<PasswordOptions>(builder.Configuration.GetSection(nameof(PasswordOptions)));

builder.Services.AddSingleton(typeof(IRepository<>), typeof(BaseRepository<>));
builder.Services.AddSingleton<IAuthService, AuthService>();
builder.Services.AddSingleton<IPasswordService, PasswordService>();
builder.Services.AddSingleton<IDataContextOptionsHelper, DataContextOptionsHelper>();
builder.Services.AddSingleton<ILogService, LogService>();
builder.Services.AddSingleton<IEducationMaterialService, EducationMaterialService>();
builder.Services.AddSingleton<ISeed, SeedData>();
builder.Services.AddSingleton<IReportService, ReportService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<DataContext>();
    context.Database.Migrate();
}

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider($"{builder.Environment.WebRootPath}/{FileHelper.FILES_PATH}"),
    RequestPath = FileHelper.FILES_PATH
});

app.UseHttpsRedirection();

app.MapControllers().RequireAuthorization();

app.Run();
