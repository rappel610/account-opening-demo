using AccountOpeningDemo.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// One named HttpClient per vendor keeps base addresses and auth headers
// scoped cleanly, rather than juggling raw HttpClient instances everywhere.
builder.Services.AddHttpClient("Plaid", client =>
{
    var baseUrl = builder.Configuration["Plaid:BaseUrl"] ?? "https://sandbox.plaid.com";
    client.BaseAddress = new Uri(baseUrl);
});

builder.Services.AddHttpClient("Stripe", client =>
{
    var baseUrl = builder.Configuration["Stripe:BaseUrl"] ?? "https://api.stripe.com/v1";
    client.BaseAddress = new Uri(baseUrl);
    var secretKey = builder.Configuration["Stripe:SecretKey"];
    client.DefaultRequestHeaders.Authorization =
        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", secretKey);
});

// In-memory only. A real implementation would persist this in SQL Server
// or similar, but a demo scoped to a weekend doesn't need durable storage
// to prove the integration logic works.
builder.Services.AddSingleton<OnboardingStatusService>();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        var allowedOrigin = builder.Configuration["Cors:AllowedOrigin"] ?? "http://localhost:5173";
        policy.WithOrigins(allowedOrigin)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthorization();
app.MapControllers();

app.Run();
