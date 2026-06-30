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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
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


// ---------- Database ----------
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


builder.Services.Configure<DataProtectionTokenProviderOptions>(
    options => options.TokenLifespan = TimeSpan.FromHours(2));



// ---------- JWT ----------
var jwtKey =
    builder.Configuration["Jwt__Secret"]
    ?? builder.Configuration["Jwt:Secret"]
    ?? throw new Exception("JWT Secret missing");


builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme =
        JwtBearerDefaults.AuthenticationScheme;

    options.DefaultChallengeScheme =
        JwtBearerDefaults.AuthenticationScheme;

})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata =
        !builder.Environment.IsDevelopment();


    options.TokenValidationParameters =
        new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,


            ValidIssuer =
                builder.Configuration["Jwt__Issuer"]
                ?? builder.Configuration["Jwt:Issuer"],


            ValidAudience =
                builder.Configuration["Jwt__Audience"]
                ?? builder.Configuration["Jwt:Audience"],


            IssuerSigningKey =
                new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwtKey)),


            ClockSkew = TimeSpan.Zero
        };

});


builder.Services.AddAuthorization();



// ---------- CORS ----------
var origins =
    builder.Configuration.GetSection("Cors__Origins")
        .Get<string[]>()
    ??
    builder.Configuration.GetSection("Cors:Origins")
        .Get<string[]>()
    ??
    new[]
    {
        "http://localhost:4200",
        "https://apollospoilers.com",
        "https://www.apollospoilers.com"
    };


builder.Services.AddCors(options =>
{
    options.AddPolicy("ApolloCors",
        policy =>
        {
            policy
            .WithOrigins(origins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();

        });
});



// ---------- AutoMapper ----------
builder.Services.AddAutoMapper(
    cfg => { },
    typeof(MappingProfile).Assembly);


builder.Services.AddFluentValidationAutoValidation();

builder.Services.AddValidatorsFromAssemblyContaining
    <RegisterRequestValidator>();



// ---------- Domain ----------
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddScoped
    <IAiKnowledgeChunkRepository, AiKnowledgeChunkRepository>();

builder.Services.AddScoped<ICatalogService, CatalogService>();
builder.Services.AddScoped<IProductAdminService, ProductAdminService>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IWishlistService, WishlistService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();



// ---------- Email ----------
builder.Services.Configure<SmtpOptions>(
    builder.Configuration.GetSection(
        SmtpOptions.SectionName));


builder.Services.AddScoped<IEmailSender, SmtpEmailSender>();

builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<ProductPrimaryImageResolver>();



// ---------- AI ----------
// Ollama Embedding
builder.Services.AddSingleton
    <IEmbeddingService, SemanticKernelEmbeddingService>();


// Qdrant
builder.Services.AddSingleton
    <IVectorStore, QdrantVectorStore>();


// Groq LLM
builder.Services.AddSingleton
    <ILlmService, SemanticKernelLlmService>();


builder.Services.AddScoped<IProductIndexer, ProductIndexer>();

builder.Services.AddScoped<IAasraChatService, AasraChatService>();

builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();




// ---------- API Version ----------
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion =
        new ApiVersion(1, 0);

    options.AssumeDefaultVersionWhenUnspecified = true;

    options.ReportApiVersions = true;

    options.ApiVersionReader =
        new UrlSegmentApiVersionReader();

})
.AddMvc()
.AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";

    options.SubstituteApiVersionInUrl = true;

});



// ---------- Controllers ----------
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1",
        new OpenApiInfo
        {
            Title = "Apollo Spoilers API",
            Version = "v1"
        });
});




var app = builder.Build();



// ---------- Render Port ----------
var port =
    Environment.GetEnvironmentVariable("PORT")
    ?? "8080";


app.Urls.Add(
    $"http://0.0.0.0:{port}");




// ---------- Migration ----------
using (var scope = app.Services.CreateScope())
{
    var db =
        scope.ServiceProvider
        .GetRequiredService<ApplicationDbContext>();

    await db.Database.MigrateAsync();
}



// ---------- Middleware ----------
app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseSerilogRequestLogging();


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    app.UseSwaggerUI();
}



app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseCors("ApolloCors");


app.UseAuthentication();

app.UseAuthorization();


app.MapControllers();



app.MapGet("/",
() => new
{
    name = "Apollo Spoilers API",
    version = "v1",
    status = "running"
});


app.Run();