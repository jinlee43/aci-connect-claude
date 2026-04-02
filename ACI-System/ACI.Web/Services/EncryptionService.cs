using System.Security.Cryptography;
using System.Text;

namespace ACI.Web.Services;

/// <summary>
/// AES-256-GCM 대칭 암호화 서비스.
/// SSN, TIN, Driver's License, Alien Number, Passport Number 등 민감 개인정보 보호용.
///
/// 저장 형식: Base64( nonce[12] || ciphertext || tag[16] )
/// </summary>
public interface IEncryptionService
{
    /// <summary>평문을 암호화하여 Base64 문자열로 반환. null → null.</summary>
    string? Encrypt(string? plaintext);

    /// <summary>Base64 암호문을 복호화하여 평문 반환. null/빈문자열 → null.</summary>
    string? Decrypt(string? ciphertext);
}

public class EncryptionService : IEncryptionService
{
    private readonly byte[] _key;

    public EncryptionService(IConfiguration config)
    {
        var keyBase64 = config["Encryption:Key"]
            ?? throw new InvalidOperationException(
                "Encryption:Key is not configured. " +
                "Add a 32-byte Base64 key to appsettings.json or environment variables.");

        _key = Convert.FromBase64String(keyBase64);
        if (_key.Length != 32)
            throw new InvalidOperationException("Encryption:Key must be exactly 32 bytes (256 bits) when Base64-decoded.");
    }

    public string? Encrypt(string? plaintext)
    {
        if (string.IsNullOrEmpty(plaintext)) return null;

        var nonce = new byte[AesGcm.NonceByteSizes.MaxSize]; // 12 bytes
        RandomNumberGenerator.Fill(nonce);

        var plaintextBytes = Encoding.UTF8.GetBytes(plaintext);
        var ciphertext     = new byte[plaintextBytes.Length];
        var tag            = new byte[AesGcm.TagByteSizes.MaxSize]; // 16 bytes

        using var aes = new AesGcm(_key, AesGcm.TagByteSizes.MaxSize);
        aes.Encrypt(nonce, plaintextBytes, ciphertext, tag);

        // 결합: nonce(12) + ciphertext(N) + tag(16)
        var combined = new byte[nonce.Length + ciphertext.Length + tag.Length];
        nonce.CopyTo(combined, 0);
        ciphertext.CopyTo(combined, nonce.Length);
        tag.CopyTo(combined, nonce.Length + ciphertext.Length);

        return Convert.ToBase64String(combined);
    }

    public string? Decrypt(string? ciphertextBase64)
    {
        if (string.IsNullOrEmpty(ciphertextBase64)) return null;

        try
        {
            var combined = Convert.FromBase64String(ciphertextBase64);

            int nonceSize      = AesGcm.NonceByteSizes.MaxSize;  // 12
            int tagSize        = AesGcm.TagByteSizes.MaxSize;     // 16
            int ciphertextSize = combined.Length - nonceSize - tagSize;

            if (ciphertextSize <= 0) return null;

            var nonce      = combined[..nonceSize];
            var ciphertext = combined[nonceSize..(nonceSize + ciphertextSize)];
            var tag        = combined[(nonceSize + ciphertextSize)..];
            var plaintext  = new byte[ciphertextSize];

            using var aes = new AesGcm(_key, tagSize);
            aes.Decrypt(nonce, ciphertext, tag, plaintext);

            return Encoding.UTF8.GetString(plaintext);
        }
        catch
        {
            // 키 불일치 또는 손상된 데이터
            return null;
        }
    }
}
