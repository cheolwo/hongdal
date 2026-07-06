using Microsoft.JSInterop;
using Hongdal.Contracts.Common.Privacy;

namespace Hongdal.Ui.Common.Areas.App.Services;

public sealed class HongdalIsmsPClientEncryptionService : IAsyncDisposable
{
    private readonly IJSRuntime jsRuntime;
    private IJSObjectReference? module;

    public HongdalIsmsPClientEncryptionService(IJSRuntime jsRuntime)
    {
        this.jsRuntime = jsRuntime;
    }

    public static PersonalDataFieldProtectionPlan BuildProtectionPlan<T>()
        => IsmsPProtectedDataAttributeReader.BuildFieldProtectionPlan(typeof(T));

    public static bool RequiresEncryptedTransport<T>()
        => BuildProtectionPlan<T>().RequiresTransportEncryption;

    public async ValueTask<IsmsPEncryptedTransportEnvelope?> EncryptJsonWhenRequiredAsync<T>(
        IsmsPClientEncryptionPublicKeyResponse publicKey,
        T value,
        string? associatedData = null)
    {
        if (!RequiresEncryptedTransport<T>())
        {
            return null;
        }

        return await EncryptJsonAsync(publicKey, value, associatedData);
    }

    public async ValueTask<IsmsPEncryptedTransportEnvelope> EncryptJsonAsync<T>(
        IsmsPClientEncryptionPublicKeyResponse publicKey,
        T value,
        string? associatedData = null)
    {
        ArgumentNullException.ThrowIfNull(publicKey);

        module ??= await jsRuntime.InvokeAsync<IJSObjectReference>(
            "import",
            "./_content/Hongdal.Ui.Common/Areas/App/js/hongdal-isms-p-transport.js");

        return await module.InvokeAsync<IsmsPEncryptedTransportEnvelope>(
            "encryptJson",
            publicKey.PublicKeyPem,
            publicKey.KeyId,
            value,
            associatedData);
    }

    public async ValueTask DisposeAsync()
    {
        if (module is null)
        {
            return;
        }

        try
        {
            await module.DisposeAsync();
        }
        catch (JSDisconnectedException)
        {
        }
    }
}
