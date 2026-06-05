using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft.SemanticKernel", Serilog.Events.LogEventLevel.Debug)
    .MinimumLevel.Override("Microsoft.SemanticKernel.Connectors", Serilog.Events.LogEventLevel.Verbose)
    .Enrich.FromLogContext()
    .WriteTo.Console(outputTemplate:
        "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File("logs/app.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);


builder.Logging.AddSerilog();
builder.Logging.AddFilter("Microsoft.SemanticKernel", LogLevel.Trace);
builder.Logging.AddFilter("Microsoft.SemanticKernel.Connectors", LogLevel.Trace);
builder.Logging.AddFilter("Microsoft", LogLevel.Warning);

builder.Host.UseSerilog();

builder.Services
    .AddOptions<AppSettings>()
    .Bind(builder.Configuration)
    .ValidateDataAnnotations()
    .ValidateOnStart();

var appSettings = builder.Configuration
    .Get<AppSettings>()!;

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


builder.Services.AddIdentity<User, IdentityRole<Guid>>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();


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

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = appSettings.Jwt.Issuer,
        ValidAudience = appSettings.Jwt.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(appSettings.Jwt.Key))
    };
})
.AddGoogle(options =>
{
    options.ClientId = appSettings.Google.ClientId;
    options.ClientSecret = appSettings.Google.ClientSecret;
    options.SignInScheme = IdentityConstants.ExternalScheme;
}); ;

builder.Services.AddControllers();

var app = builder.Build();
app.UseMiddleware<ErrorHandlerMiddleware>();

app.UseHttpsRedirection();

app.UseAuthentication(); // Must come BEFORE UseAuthorization
app.UseAuthorization();

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
