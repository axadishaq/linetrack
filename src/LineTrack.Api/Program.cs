var builder = WebApplication.CreateBuilder(args);

// ---- Services: register what the app needs in the DI container ----
// (Like Spring's @Bean / NestJS providers: the framework creates and injects them.)
builder.Services.AddControllers();
builder.Services.AddOpenApi();       // generates the OpenAPI JSON; Scalar UI comes later
builder.Services.AddHealthChecks();  // PostgreSQL check is added once the database exists

var app = builder.Build();

// ---- Middleware pipeline: every request passes through these, in order ----
// (Like Express app.use(...) chains.)
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();                // serves /openapi/v1.json
}

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");      // Kubernetes probes will call this in Week 4

app.Run();
