using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using UniversiteDomain.DataAdapters.DataAdaptersFactory;
using UniversiteDomain.JeuxDeDonnees;
using UniversiteEFDataProvider.Data;
using UniversiteEFDataProvider.Entities;
using UniversiteEFDataProvider.RepositoryFactories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Projet Universite", Version = "v1" });
    
    // Configuration du bouton "Authorize" (Cadenas)
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Entrez votre token JWT ici."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
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

builder.Services.AddLogging(options =>
{
    options.ClearProviders();
    options.AddConsole();
});

// Configuration de la connexion à MySql
String connectionString = builder.Configuration.GetConnectionString("MySqlConnection") 
                          ?? throw new InvalidOperationException("Connection string 'MySqlConnection' not found.");

builder.Services.AddDbContext<UniversiteDbContext>(options => options.UseMySQL(connectionString));

// La factory est rajoutée dans les services
builder.Services.AddScoped<IRepositoryFactory, RepositoryFactory>();

builder.Services.AddIdentityCore<UniversiteUser>(options=>
    { 
        // Configuration souple du mot de passe pour le dev
        options.Password.RequiredLength = 4;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireLowercase = false;
        options.Password.RequireDigit = false;
        
        options.User.RequireUniqueEmail = true;
    })
    .AddRoles<UniversiteRole>()
    .AddEntityFrameworkStores<UniversiteDbContext>()
    .AddApiEndpoints()
    .AddDefaultTokenProviders();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });

var app = builder.Build();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization(); 

app.MapControllers();

app.UseSwagger();
app.UseSwaggerUI();

using(var scope = app.Services.CreateScope())
{
    // On récupère le logger proprement dans le scope
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<BdBuilder>>();
    logger.LogInformation("Lancement de l'initialisation...");

    try 
    {
        var repositoryFactory = scope.ServiceProvider.GetRequiredService<IRepositoryFactory>();   
        
        // Le BdBuilder s'occupe de supprimer/créer la base (RegenererBdAsync) et de charger les données
        BdBuilder seedBD = new BasicBdBuilder(repositoryFactory);
        await seedBD.BuildUniversiteBdAsync();
        
        logger.LogInformation("Base de données initialisée et chargée avec succès (Sécurité incluse).");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Erreur lors de l'initialisation de la BD.");
    }
}

app.Run();