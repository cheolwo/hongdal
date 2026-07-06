using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Hongdal.Contracts.Common.Hr;
using Hongdal.Contracts.Common.Privacy;
using Hongdal.Contracts.Shipper.Request;
using 홍달.Infrastructure.Security;

namespace Hongdal.Tests.Infrastructure.Security;

public sealed class IsmsPProtectedDataSecurityServiceTests
{
    [Fact]
    public void EncryptAtRest_UsesAes256GcmPrefixAndDoesNotReturnPlainText()
    {
        var service = new AesGcmIsmsPProtectedDataCryptoService(CreateOptions());

        var protectedValue = service.EncryptAtRest(PersonalDataFieldKey.BankAccountNumber, "123-456-7890");

        Assert.Equal("AES-256-GCM", protectedValue.AlgorithmCode);
        Assert.StartsWith(AesGcmIsmsPProtectedDataCryptoService.EncryptionPrefix, protectedValue.StoredValue);
        Assert.DoesNotContain("123-456-7890", protectedValue.StoredValue);
    }

    [Fact]
    public void DecryptAtRest_RecoversEncryptedValue()
    {
        var service = new AesGcmIsmsPProtectedDataCryptoService(CreateOptions());
        var protectedValue = service.EncryptAtRest(PersonalDataFieldKey.BankAccountNumber, "123-456-7890");

        var plainText = service.DecryptAtRest(PersonalDataFieldKey.BankAccountNumber, protectedValue.StoredValue);

        Assert.Equal("123-456-7890", plainText);
    }

    [Fact]
    public void PrepareForStorage_EncryptsAttributedDtoStringProperties()
    {
        var service = new IsmsPProtectedDataStorePreparationService(
            new AesGcmIsmsPProtectedDataCryptoService(CreateOptions()));
        var request = new HrEmploymentContractDraftRequest
        {
            WorkerUserId = "worker-1",
            WorkerName = "홍길동",
            PaymentMethod = HrPaymentMethods.BankTransfer,
            AccountNumber = "123-456-7890",
            AccountHolderName = "홍길동"
        };

        var result = service.PrepareForStorage(request);

        Assert.NotSame(request, result.Value);
        Assert.Equal("123-456-7890", request.AccountNumber);
        Assert.StartsWith(AesGcmIsmsPProtectedDataCryptoService.EncryptionPrefix, result.Value.AccountNumber);
        Assert.Contains(result.ProtectedMembers, x =>
            x.PropertyName == nameof(HrEmploymentContractDraftRequest.AccountNumber) &&
            x.AlgorithmCode == AesGcmIsmsPProtectedDataCryptoService.EncryptionAlgorithmCode);
    }

    [Fact]
    public void PrepareForStorage_ProtectsShipperContactPhoneNumber()
    {
        var service = new IsmsPProtectedDataStorePreparationService(
            new AesGcmIsmsPProtectedDataCryptoService(CreateOptions()));
        var contact = new ContactDTO
        {
            이름 = "상차 담당자",
            전화번호 = "010-1111-2222"
        };

        var result = service.PrepareForStorage(contact);

        Assert.StartsWith(AesGcmIsmsPProtectedDataCryptoService.EncryptionPrefix, result.Value.전화번호);
        Assert.Contains(result.ProtectedMembers, x => x.FieldKey == PersonalDataFieldKey.PhoneNumber);
    }

    [Fact]
    public void PrepareForStorage_DoesNotEncryptClassifiedOnlyFields()
    {
        var service = new IsmsPProtectedDataStorePreparationService(
            new AesGcmIsmsPProtectedDataCryptoService(CreateOptions()));
        var request = new ClassifiedOnlyStorageFixture
        {
            PaymentMethod = "card"
        };

        var result = service.PrepareForStorage(request);

        Assert.Equal("card", result.Value.PaymentMethod);
        Assert.Contains(result.ProtectedMembers, x =>
            x.FieldKey == PersonalDataFieldKey.PaymentMethod &&
            x.AlgorithmCode == "PLAIN-CLASSIFIED");
    }

    [Fact]
    public void PrepareForStorage_HashesEvidenceOnlyFields()
    {
        var service = new IsmsPProtectedDataStorePreparationService(
            new AesGcmIsmsPProtectedDataCryptoService(CreateOptions()));
        var request = new EvidenceHashStorageFixture
        {
            IpAddress = "203.0.113.7"
        };

        var result = service.PrepareForStorage(request);

        Assert.StartsWith(AesGcmIsmsPProtectedDataCryptoService.HashPrefix, result.Value.IpAddress);
        Assert.DoesNotContain("203.0.113.7", result.Value.IpAddress);
        Assert.Contains(result.ProtectedMembers, x =>
            x.FieldKey == PersonalDataFieldKey.IpAddress &&
            x.AlgorithmCode == AesGcmIsmsPProtectedDataCryptoService.HashAlgorithmCode);
    }

    [Fact]
    public void PrepareForResponse_MasksEncryptedValuesByDefault()
    {
        var cryptoService = new AesGcmIsmsPProtectedDataCryptoService(CreateOptions());
        var responseService = new IsmsPProtectedDataResponsePreparationService(cryptoService);
        var stored = new BankAccountStorageFixture
        {
            AccountNumber = cryptoService.EncryptAtRest(
                PersonalDataFieldKey.BankAccountNumber,
                "123-456-7890").StoredValue
        };

        var result = responseService.PrepareForResponse(stored);

        Assert.Equal("****7890", result.Value.AccountNumber);
        Assert.Contains(result.ProtectedMembers, x =>
            x.FieldKey == PersonalDataFieldKey.BankAccountNumber &&
            x.WasDecrypted &&
            x.WasMasked);
    }

    [Fact]
    public void PrepareForResponse_RevealsEncryptedValuesWhenExplicitlyAllowed()
    {
        var cryptoService = new AesGcmIsmsPProtectedDataCryptoService(CreateOptions());
        var responseService = new IsmsPProtectedDataResponsePreparationService(cryptoService);
        var stored = new BankAccountStorageFixture
        {
            AccountNumber = cryptoService.EncryptAtRest(
                PersonalDataFieldKey.BankAccountNumber,
                "123-456-7890").StoredValue
        };

        var result = responseService.PrepareForResponse(stored, revealProtectedValues: true);

        Assert.Equal("123-456-7890", result.Value.AccountNumber);
        Assert.Contains(result.ProtectedMembers, x =>
            x.FieldKey == PersonalDataFieldKey.BankAccountNumber &&
            x.WasDecrypted &&
            !x.WasMasked);
    }

    [Fact]
    public void DecryptTransportEnvelope_RecoversClientEncryptedJsonPayload()
    {
        using var rsa = RSA.Create(2048);
        var publicKeyPem = rsa.ExportSubjectPublicKeyInfoPem();
        var privateKeyPem = rsa.ExportPkcs8PrivateKeyPem();
        var options = CreateOptions(publicKeyPem, privateKeyPem);
        var service = new RsaOaepAesGcmClientTransportProtectionService(options);
        var envelope = EncryptForServer(
            publicKeyPem,
            service.GetPublicKey().KeyId,
            "{\"phone\":\"010-1111-2222\"}",
            "hr-contract-draft");

        var decrypted = service.Decrypt(envelope);

        Assert.Equal("{\"phone\":\"010-1111-2222\"}", decrypted.JsonPayload);
    }

    private static IsmsPEncryptedTransportEnvelope EncryptForServer(
        string publicKeyPem,
        string keyId,
        string json,
        string associatedData)
    {
        var aesKey = RandomNumberGenerator.GetBytes(32);
        var nonce = RandomNumberGenerator.GetBytes(12);
        var plainText = Encoding.UTF8.GetBytes(json);
        var cipherText = new byte[plainText.Length];
        var tag = new byte[16];
        var aad = Encoding.UTF8.GetBytes(associatedData);

        using (var aes = new AesGcm(aesKey, tag.Length))
        {
            aes.Encrypt(nonce, plainText, cipherText, tag, aad);
        }

        using var rsa = RSA.Create();
        rsa.ImportFromPem(publicKeyPem);
        var encryptedKey = rsa.Encrypt(aesKey, RSAEncryptionPadding.OaepSHA256);
        var cipherTextWithTag = cipherText.Concat(tag).ToArray();

        return new IsmsPEncryptedTransportEnvelope(
            keyId,
            IsmsPTransportEncryptionAlgorithmCode.RsaOaepSha256Aes256Gcm,
            Convert.ToBase64String(encryptedKey),
            Convert.ToBase64String(nonce),
            Convert.ToBase64String(cipherTextWithTag),
            associatedData);
    }

    private static IOptions<IsmsPProtectedDataOptions> CreateOptions(
        string? publicKeyPem = null,
        string? privateKeyPem = null)
        => Options.Create(new IsmsPProtectedDataOptions
        {
            Aes256GcmKeyBase64 = Convert.ToBase64String(Enumerable.Range(1, 32).Select(x => (byte)x).ToArray()),
            HashSalt = "test-salt",
            TransportKeyId = "test-key",
            TransportPublicKeyPem = publicKeyPem,
            TransportPrivateKeyPem = privateKeyPem
        });

    private sealed class ClassifiedOnlyStorageFixture
    {
        [IsmsPProtectedData(PersonalDataFieldKey.PaymentMethod, "payment method display")]
        public string PaymentMethod { get; set; } = string.Empty;
    }

    private sealed class EvidenceHashStorageFixture
    {
        [IsmsPProtectedData(PersonalDataFieldKey.IpAddress, "security audit ip")]
        public string IpAddress { get; set; } = string.Empty;
    }

    private sealed class BankAccountStorageFixture
    {
        [IsmsPProtectedData(PersonalDataFieldKey.BankAccountNumber, "settlement account")]
        public string AccountNumber { get; set; } = string.Empty;
    }
}
