using System;
using System.Collections.Generic;
using System.Text;
using AircraftMaintenance.Api.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SmartRAG.Enums;
using SmartRAG.Extensions;
using SmartRAG.Interfaces.FileWatcher;
using SmartRAG.Models.Configuration;
using SmartRAG.Services.FileWatcher;

var builder = WebApplication.CreateBuilder(args);

// Step 1: Add database (SQL Server from appsettings)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Step 2: Add Identity (users, roles, JWT login)
builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// Step 3: Configure JWT authentication
var jwtKey = builder.Configuration["Jwt:Key"];
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
        // Keep claims from the JWT token (like name, email, role)
        options.MapInboundClaims = false;
    });

// Step 4: Add SmartRAG (Qdrant vector DB + file watcher)
builder.Services.AddSmartRag(builder.Configuration, options =>
{
    options.StorageProvider = StorageProvider.Qdrant;
});

// Step 5: Add API controllers
builder.Services.AddControllers();

// Step 6: Add Swagger (API documentation)
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "AircraftMaintenance.Api", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT token here."
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddOpenApi();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

// Step 7: Add the File Watcher service (manually registered since AddSmartRag doesn't expose it directly)
builder.Services.AddScoped<IFileWatcherService, FileWatcherService>();

// Step 8: Build the app
var app = builder.Build();

// Step 8: Start the file watcher
// CRITICAL: Keep the scope alive (no 'using' block) so the watcher doesn't get disposed.
var watcherScope = app.Services.CreateScope();
var fileWatcher = watcherScope.ServiceProvider.GetRequiredService<IFileWatcherService>();
var folders = builder.Configuration.GetSection("SmartRAG:FileWatcher:Folders").Get<List<WatchedFolderConfig>>();
if (folders != null)
{
    foreach (var folder in folders)
    {
        await fileWatcher.StartWatchingAsync(folder);
    }
}

// Step 9: Apply database migrations and seed data
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
    await DbSeeder.SeedAsync(app.Services);
}

// Step 10: Configure the HTTP pipeline
app.UseCors("AllowAll");
app.UseSwagger();
app.UseSwaggerUI();
if (app.Environment.IsDevelopment()) app.MapOpenApi();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
