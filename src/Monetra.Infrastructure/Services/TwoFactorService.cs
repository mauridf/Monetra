using Monetra.Core.Interfaces;
using OtpNet;

namespace Monetra.Infrastructure.Services;

public class TwoFactorService : ITwoFactorService
{
    public string GenerateSecretKey()
    {
        var secretKey = KeyGeneration.GenerateRandomKey(20);
        return Base32Encoding.ToString(secretKey);
    }

    public string GetQrCodeUri(string secretKey, string email, string issuer)
    {
        return $"otpauth://totp/{issuer}:{email}?secret={secretKey}&issuer={issuer}&algorithm=SHA1&digits=6&period=30";
    }

    public bool VerifyCode(string secretKey, string code)
    {
        var encodedSecret = Base32Encoding.ToBytes(secretKey);
        var totp = new Totp(encodedSecret);
        return totp.VerifyTotp(code, out _, new VerificationWindow(1, 1));
    }
}
