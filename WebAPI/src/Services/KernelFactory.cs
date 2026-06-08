namespace WebAPI.Services;

public interface IKernelFactory
{
    Kernel Create(Guid sessionId);
}

public sealed class KernelFactory(
    ILoggerFactory loggerFactory,
    IChatSessionCommandService command,
    IChatSessionQueryService query) : IKernelFactory
{

    public Kernel Create(Guid sessionId)
    {
        var builder = Kernel.CreateBuilder();

        var logger = loggerFactory.CreateLogger<CVFunctions>();

        builder.Plugins.AddFromObject(
            new CVFunctions(
                command,
                query,
                sessionId, logger)
            );

        builder.AddOpenAIChatCompletion(
            modelId: "google/gemini-2.5-flash-lite",
            apiKey: Environment.GetEnvironmentVariable("OPENROUTER_API_KEY"),
            endpoint: new Uri("https://openrouter.ai/api/v1"),
            httpClient: new HttpClient
            {
                DefaultRequestHeaders =
                {
                    { "HTTP-Referer", "http://localhost:5044" },
                    { "X-Title", "CV-App" }
                }
            });

        return builder.Build();
    }
}
