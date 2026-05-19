var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy =>
        {
            policy
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowAnyOrigin();
        });
});
builder.Logging.AddOpenTelemetry(logging =>
{
    logging.IncludeFormattedMessage = true;
    logging.IncludeScopes = true;
    logging.AddOtlpExporter();
});

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/app.log", rollingInterval: RollingInterval.Day)
    .Enrich.FromLogContext()
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddKernel()
    .AddOpenAIChatCompletion(
        modelId: "openai/gpt-oss-120b:free",
        apiKey: Environment.GetEnvironmentVariable("OPENROUTER_API_KEY"),
        endpoint: new Uri("https://openrouter.ai/api/v1"),
        httpClient: new HttpClient
        {
            DefaultRequestHeaders =
            {
                { "HTTP-Referer", "http://localhost:5044" },
                { "X-Title", "CV-App" },
            },
        }
    );

var dataSourceBuilder = new NpgsqlDataSourceBuilder(builder.Configuration.GetConnectionString("DefaultConnection"));

dataSourceBuilder.EnableDynamicJson();

var dataSource = dataSourceBuilder.Build();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(dataSource));

builder.Services.AddControllers();
builder.Services.AddScoped<IChatSessionService, ChatSessionService>();
builder.Services.AddOpenApi();

builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<AwardRepository>();
builder.Services.AddScoped<CertificateRepository>();
builder.Services.AddScoped<EducationRepository>();
builder.Services.AddScoped<LanguageRepository>();
builder.Services.AddScoped<ProfileRepository>();
builder.Services.AddScoped<ProjectRepository>();
builder.Services.AddScoped<SkillRepository>();
builder.Services.AddScoped<UserProfileRepository>();
builder.Services.AddScoped<WorkExperienceRepository>();

builder.Services.AddOpenTelemetry()
    .WithTracing(tracing =>
    {
        tracing
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddEntityFrameworkCoreInstrumentation()
            .AddOtlpExporter();
    })
    .WithMetrics(metrics =>
    {
        metrics
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddRuntimeInstrumentation()
            .AddOtlpExporter();
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.UseCors("AllowFrontend");
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapOpenApi("/openapi/{documentName}.yaml");

app.UseRouting();
app.MapControllers();

await app.RunAsync();
