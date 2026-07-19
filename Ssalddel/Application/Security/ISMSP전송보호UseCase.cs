using Ssalddel.ApiMetadata;
using Ssalddel.Contracts.Common.Privacy;
using Ssalddel.Services.Security;
using 살뜰.Infrastructure.Security;

namespace Ssalddel.Application.Security;

public interface IISMSP전송보호UseCase
{
    Task<IsmsPClientEncryptionPublicKeyResponse> 공개키발급Async(CancellationToken cancellationToken);
}

[SsalddelApiWorkflow(SsalddelWorkflow.DomesticTransport)]
[SsalddelUseCase("ISMS-P 전송 보호", Summary = "클라이언트 암호화 공개키를 발급하고 전송 키 활성 상태를 기록합니다.")]
[SsalddelUseCaseActor(SsalddelActor.PlatformOperator)]
public sealed class ISMSP전송보호UseCase : IISMSP전송보호UseCase
{
    private readonly IIsmsPClientTransportProtectionService _protectionService;
    private readonly IIsmsPTransportKeyStatusStore _keyStatusStore;

    public ISMSP전송보호UseCase(
        IIsmsPClientTransportProtectionService protectionService,
        IIsmsPTransportKeyStatusStore keyStatusStore)
    {
        _protectionService = protectionService;
        _keyStatusStore = keyStatusStore;
    }

    public async Task<IsmsPClientEncryptionPublicKeyResponse> 공개키발급Async(CancellationToken cancellationToken)
    {
        var publicKey = _protectionService.GetPublicKey();
        await _keyStatusStore.MarkActiveAsync(publicKey, cancellationToken);
        return publicKey;
    }
}
