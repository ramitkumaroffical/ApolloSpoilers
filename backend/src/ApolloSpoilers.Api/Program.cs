using System.Reflection;
using System.Text;
using Asp.Versioning;
using ApolloSpoilers.Application.Interfaces;
using ApolloSpoilers.Application.Mapping;
using ApolloSpoilers.Application.Services;
using ApolloSpoilers.Application.Validation;
using ApolloSpoilers.Domain.Entities;
using ApolloSpoilers.Domain.Interfaces;
using ApolloSpoilers.Domain.Interfaces.Ai;
using ApolloSpoilers.Infrastructure.Ai;
using ApolloSpoilers.Infrastructure.Email;
using ApolloSpoilers.Infrastructure.Identity;
using ApolloSpoilers.Infrastructure.Persistence;
using ApolloSpoilers.Infrastructure.Persistence.Repositories;
using ApolloSpoilers.Api.Middlewares;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// ---------- Serilog ----------
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/apollo-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();
builder.Host.UseSerilog();

// ---------- DbContext ----------
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sql => sql.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

// ---------- Identity ----------
builder.Services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
    {
        options.Password.RequireDigit = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequiredLength = 8;
        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// ---------- Identity token lifespan (password-reset tokens) ----------
// Default is 1 hour; extend to 2 hours for a more forgiving reset window.
builder.Services.Configure<DataProtectionTokenProviderOptions>(
    options => options.TokenLifespan = TimeSpan.FromHours(2));

// ---------- JWT ----------
var jwtKey = builder.Configuration["Jwt:Secret"]!;
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,    
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ClockSkew = TimeSpan.Zero
    };

});
builder.Services.AddAuthorization();

// ---------- CORS ----------
var origins = builder.Configuration.GetSection("Cors:Origins").Get<string[]>() ?? new[] { "http://localhost:4200" };
builder.Services.AddCors(o => o.AddPolicy("ApolloCors", policy =>
    policy.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod().AllowCredentials()));

// ---------- AutoMapper, validators ----------
builder.Services.AddAutoMapper(cfg => { }, typeof(MappingProfile).Assembly);
builder.Services.AddAutoMapper(cfg => cfg.AddMaps(typeof(MappingProfile).Assembly));
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<RegisterRequestValidator>();

// ---------- Unit of Work & domain services ----------
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IAiKnowledgeChunkRepository, AiKnowledgeChunkRepository>();
builder.Services.AddScoped<ICatalogService, CatalogService>();
builder.Services.AddScoped<IProductAdminService, ProductAdminService>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IWishlistService, WishlistService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

// ---------- Email (SMTP) ----------
builder.Services.Configure<SmtpOptions>(builder.Configuration.GetSection(SmtpOptions.SectionName));
builder.Services.AddScoped<IEmailSender, SmtpEmailSender>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ProductPrimaryImageResolver>();

// ---------- AI services ----------
builder.Services.AddSingleton<IEmbeddingService, SemanticKernelEmbeddingService>();
builder.Services.AddSingleton<IVectorStore, QdrantVectorStore>();
builder.Services.AddSingleton<ILlmService, SemanticKernelLlmService>();
builder.Services.AddScoped<IProductIndexer, ProductIndexer>();
builder.Services.AddScoped<IAasraChatService, AasraChatService>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

// ---------- API versioning ----------
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
}).AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

// ---------- MVC + Swagger ----------
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Apollo Spoilers API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization. Example: \"Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
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

var app = builder.Build();
app.UseStaticFiles();

// ---------- Migrate + seed ----------
using (var scope = app.Services.CreateScope())
{
    var sp = scope.ServiceProvider;
    try
    {
        var db = sp.GetRequiredService<ApplicationDbContext>();
        await db.Database.MigrateAsync();

        var userManager = sp.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = sp.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        await ApplicationDbContextSeed.SeedAsync(db, userManager, roleManager);
    }
    catch (Exception ex)
    {
        Log.Logger.Error(ex, "Database migration/seed failed.");
    }
}

// ---------- Populate the Aasra RAG knowledge base (Qdrant) ----------
// Runs once on startup in the background so the API stays responsive.
// Idempotent: re-embedding the catalog simply overwrites existing points.
// If Ollama/Qdrant are unavailable, it fails gracefully without crashing startup.
_ = Task.Run(async () =>
{
    try
    {
        using var scope = app.Services.CreateScope();
        var indexer = scope.ServiceProvider.GetRequiredService<IProductIndexer>();
        Log.Logger.Information("Aasra: starting catalog reindex into Qdrant...");
        await indexer.ReindexAllAsync();
        Log.Logger.Information("Aasra: catalog reindex complete.");
    }
    catch (Exception ex)
    {
        Log.Logger.Error(ex, "Aasra: catalog reindex failed (Qdrant/Ollama may be unavailable). The assistant will still run but with an empty knowledge base.");
    }
});

// ---------- Middleware pipeline ----------
app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseCors("ApolloCors");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.MapGet("/", () => new { name = "Apollo Spoilers API", version = "v1", status = "running" });

app.Run();
