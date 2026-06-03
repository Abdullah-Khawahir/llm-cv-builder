using System.Security.Cryptography;
using Minio.DataModel.Args;
using Minio.Exceptions;

namespace WebAPI.Controllers;

[ApiController]
[Route("api/cv")]
public sealed class CVController(ApplicationDbContext db, IMinioClient minio) : ControllerBase
{
    private readonly ApplicationDbContext _db = db;
    private readonly IMinioClient _minio = minio;

    [HttpGet("preview/{id:guid}")]
    public async Task<IResult> PreviewCV([FromRoute] Guid id)
    {
        var session = await _db.ChatSessions
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id)
            ?? throw AppException.NotFound($"Session not found: {id}");

        var htmlHash = ComputeHash(session.HtmlDocument);
        var fileName = $"{session.Id}_{htmlHash}.pdf";

        var memory = new MemoryStream();
        var exists = true;

        try
        {
            await _minio.GetObjectAsync(
                new GetObjectArgs()
                    .WithBucket("pdfs")
                    .WithObject(fileName)
                    .WithCallbackStream(s => s.CopyTo(memory)));

            memory.Position = 0;
        }
        catch (ObjectNotFoundException)
        {
            exists = false;
        }

        if (!exists)
        {
            memory = await PdfGenerator.GenerateAsync(session.HtmlDocument);

            await _minio.PutObjectAsync(
                new PutObjectArgs()
                    .WithBucket("pdfs")
                    .WithObject(fileName)
                    .WithStreamData(memory)
                    .WithObjectSize(memory.Length)
                    .WithContentType("application/pdf"));

            memory.Position = 0;
        }

        return Results.File(memory, "application/pdf");
    }



    private static string ComputeHash(string html)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(html.Trim()));
        return Convert.ToHexString(bytes)[..16];
    }
}
