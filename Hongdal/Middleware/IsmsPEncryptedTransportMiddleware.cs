using System.Text;
using System.Text.Json;
using Hongdal.Contracts.Common.Privacy;
using Hongdal.Services.Security;
using Microsoft.AspNetCore.Mvc;
using 홍달.Infrastructure.Security;

namespace Hongdal.Middleware;

public sealed class IsmsPEncryptedTransportMiddleware
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly RequestDelegate next;
    private readonly ILogger<IsmsPEncryptedTransportMiddleware> logger;

    public IsmsPEncryptedTransportMiddleware(
        RequestDelegate next,
        ILogger<IsmsPEncryptedTransportMiddleware> logger)
    {
        this.next = next;
        this.logger = logger;
    }

    public async Task InvokeAsync(
        HttpContext context,
        IIsmsPClientTransportProtectionService protectionService,
        IIsmsPTransportKeyStatusStore keyStatusStore)
    {
        if (!CanInspect(context.Request))
        {
            await next(context);
            return;
        }

        context.Request.EnableBuffering();

        var originalBody = context.Request.Body;
        string body;
        using (var reader = new StreamReader(
            originalBody,
            Encoding.UTF8,
            detectEncodingFromByteOrderMarks: false,
            bufferSize: 4096,
            leaveOpen: true))
        {
            body = await reader.ReadToEndAsync(context.RequestAborted);
        }

        originalBody.Position = 0;

        if (string.IsNullOrWhiteSpace(body) || !LooksLikeEnvelope(body))
        {
            await next(context);
            return;
        }

        IsmsPEncryptedTransportEnvelope? envelope;
        try
        {
            envelope = JsonSerializer.Deserialize<IsmsPEncryptedTransportEnvelope>(body, JsonOptions);
        }
        catch (JsonException ex)
        {
            await WriteBadRequestAsync(context, "Invalid ISMS-P encrypted transport envelope.", ex);
            return;
        }

        if (envelope is null)
        {
            await WriteBadRequestAsync(context, "Empty ISMS-P encrypted transport envelope.");
            return;
        }

        var isActiveKey = await keyStatusStore.IsActiveAsync(
            envelope.KeyId,
            envelope.AlgorithmCode,
            context.RequestAborted);
        if (!isActiveKey)
        {
            await WriteBadRequestAsync(context, "Inactive or expired ISMS-P transport key.");
            return;
        }

        IsmsPDecryptedTransportPayload decrypted;
        try
        {
            decrypted = protectionService.Decrypt(envelope);
        }
        catch (Exception ex) when (ex is InvalidOperationException or ArgumentException or FormatException or System.Security.Cryptography.CryptographicException)
        {
            await WriteBadRequestAsync(context, "Unable to decrypt ISMS-P encrypted transport envelope.", ex);
            return;
        }

        var decryptedBytes = Encoding.UTF8.GetBytes(decrypted.JsonPayload);
        await using var decryptedBody = new MemoryStream(decryptedBytes);

        context.Request.Body = decryptedBody;
        context.Request.ContentLength = decryptedBytes.Length;
        context.Request.Headers["X-Hongdal-IsmsP-Transport"] = "decrypted";

        await next(context);

        context.Request.Body = originalBody;
    }

    private static bool CanInspect(HttpRequest request)
    {
        if (!HttpMethods.IsPost(request.Method) &&
            !HttpMethods.IsPut(request.Method) &&
            !HttpMethods.IsPatch(request.Method) &&
            !HttpMethods.IsDelete(request.Method))
        {
            return false;
        }

        return request.HasJsonContentType();
    }

    private static bool LooksLikeEnvelope(string body)
    {
        try
        {
            using var document = JsonDocument.Parse(body);
            var root = document.RootElement;

            return root.ValueKind == JsonValueKind.Object &&
                HasProperty(root, "keyId") &&
                HasProperty(root, "algorithmCode") &&
                HasProperty(root, "encryptedKeyBase64") &&
                HasProperty(root, "nonceBase64") &&
                HasProperty(root, "cipherTextBase64");
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private static bool HasProperty(JsonElement element, string name)
        => element.TryGetProperty(name, out _) ||
            element.TryGetProperty(char.ToUpperInvariant(name[0]) + name[1..], out _);

    private async Task WriteBadRequestAsync(
        HttpContext context,
        string title,
        Exception? exception = null)
    {
        if (exception is not null)
        {
            logger.LogWarning(exception, "ISMS-P encrypted transport request was rejected.");
        }

        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        context.Response.ContentType = "application/problem+json";
        await context.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Title = title,
            Status = StatusCodes.Status400BadRequest
        });
    }
}
