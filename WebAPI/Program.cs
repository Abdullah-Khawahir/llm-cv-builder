var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddOptions<AppSettings>()
    .Bind(builder.Configuration)
    .ValidateDataAnnotations()
    .ValidateOnStart();

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

builder.Services.AddMinio((minio) =>
        minio
        .WithCredentials("minioadmin", "minioadmin")
        .WithEndpoint("localhost:9000")
        .WithSSL(false)
        .Build()
        );

builder.Services.AddKernel()
    .AddOpenAIChatCompletion(
        modelId: "gemini-3.1-flash-lite",
        apiKey: Environment.GetEnvironmentVariable("CVAPP_GEMINI_API_KEY")!,
        // endpoint: new Uri("https://generativelanguage.googleapis.com/v1beta/models"),
        httpClient: new HttpClient
        {
            DefaultRequestHeaders =
            {
                { "HTTP-Referer", "http://localhost:5044" },
                { "X-Title", "CV-App" },
                {"X-OpenRouter-Title", "CV-APP"},
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

builder.Services.AddControllers();

var app = builder.Build();

app.UseCors("AllowFrontend");
app.UseMiddleware<ErrorHandlerMiddleware>();
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
