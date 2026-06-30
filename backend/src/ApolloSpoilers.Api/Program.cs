using System.Net.Http.Headers;

var builder = WebApplication.CreateBuilder(args);

// =============================================================
// 1. AI SERVICES CONFIGURATION (Groq & Nomic Cloud)
// =============================================================

// CHAT LLM: Groq Client Setup
builder.Services.AddHttpClient("GroqChatClient", client =>
{
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
    var baseUrl = builder.Configuration["Ai__Embedding__BaseUrl"] ?? "https://api-atlas.nomic.ai/v1";
    var apiKey = builder.Configuration["Ai__Embedding__ApiKey"];

    client.BaseAddress = new Uri(baseUrl);

    if (!string.IsNullOrEmpty(apiKey))
    {
        // Ollama me header nahi chahiye tha, par Nomic Cloud me Bearer Token zaroori hai
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
    }
});

// =============================================================
// 2. DATABASE CONFIGURATIONS (SQL Server & Qdrant)
// =============================================================

// SQL Server Connection
var connectionString = builder.Configuration["ConnectionStrings__DefaultConnection"];
// builder.Services.AddDbContext<YourDbContext>(options => options.UseSqlServer(connectionString)); // Apne DbContext ke hisab se uncomment karein

// Qdrant Configuration Setup
// Yahan aapka jo bhi Qdrant client inject hota hai, use rehne dein. 
// Kyunki endpoint aur API key seedhe config se read ho rhi hai, usme issue nahi aayega.


// =============================================================
// 3. CORS CONFIGURATION
// =============================================================
var allowedOrigins = builder.Configuration.GetSection("Cors__Origins").Get<string[]>() ?? new string[] { };

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

// Middleware pipeline
app.UseCors();
app.UseAuthorization();
app.MapControllers();

app.Run();