using ApolloSpoilers.Api.Middlewares;
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
using Asp.Versioning;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;

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
builder.Services.Configure<DataProtectionTokenProviderOptions>(
    options => options.TokenLifespan = TimeSpan.FromHours(2));

// ---------- JWT ----------
// FIX: Environment variables compatibility for JWT Section
var jwtKey = builder.Configuration["Jwt__Secret"] ?? builder.Configuration["Jwt:Secret"]!;
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
        ValidIssuer = builder.Configuration["Jwt__Issuer"] ?? builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt__Audience"] ?? builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ClockSkew = TimeSpan.Zero
    };
});
builder.Services.AddAuthorization();


// CHAT LLM: Groq Client Setup
builder.Services.AddHttpClient("GroqChatClient", client =>
{
    // FIX: Env variable double underscore fallback
    var baseUrl = builder.Configuration["Ai__Llm__BaseUrl"] ?? "https://api.groq.com/openai/v1";
    var apiKey = builder.Configuration["Ai__Llm__ApiKey"];

    client.BaseAddress = new Uri(baseUrl);

    if (!string.IsNullOrEmpty(apiKey))
    {
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
    }
});

// EMBEDDING: Nomic Cloud Client Setup
builder.Services.AddHttpClient("NomicEmbeddingClient", client =>
{
    // FIX: Env variable double underscore fallback
    var baseUrl = builder.Configuration["Ai__Embedding__BaseUrl"] ?? "https://api-atlas.nomic.ai/v1";
    var apiKey = builder.Configuration["Ai__Embedding__ApiKey"];

    client.BaseAddress = new Uri(baseUrl);

    if (!string.IsNullOrEmpty(apiKey))
    {
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
    }
});


// ---------- CORS Configuration ----------
// FIX: Config Section reads both Cors__Origins and Cors:Origins for Render/Vercel support
var originsSection = builder.Configuration.GetSection("Cors__Origins").Get<string[]>()
                     ?? builder.Configuration.GetSection("Cors:Origins").Get<string[]>();

var origins = originsSection ?? new[]
{
    "http://localhost:4200",
    "https://apollospoilers.com",
    "https://www.apollospoilers.com"
};

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular",
        policy =>
        {
            policy
                .WithOrigins(origins) // FIX: Uses dynamic array from config instead of hardcoded strings
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        });
});


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


builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
})
.AddMvc()
.AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

// ---------- Controllers ----------
builder.Services.AddControllers();


// ===================== BUILD =====================
var app = builder.Build();


// ---------- Port binding for Render ----------
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
app.Urls.Add($"http://0.0.0.0:{port}");

app.UseHttpsRedirection();

app.UseCors("AllowAngular");

app.UseStaticFiles();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();