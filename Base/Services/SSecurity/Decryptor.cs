namespace Base.Services.SSecurity;

using System.Security.Cryptography;

internal static class Decryptor
{
    private static string _decrypt(string key, string iv, string cipherText)
    {
        if (key == null || iv == null || string.IsNullOrEmpty(cipherText)) { throw new ArgumentNullException("Arguments for encryption cannot be null"); }

        cipherText = cipherText.Trim('"').Trim();

        try
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = Convert.FromHexString(key);
                aes.IV = Convert.FromHexString(iv);

                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                using (MemoryStream ms = new MemoryStream(Convert.FromBase64String(cipherText)))
                {
                    using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader sr = new StreamReader(cs)) { string response = sr.ReadToEnd(); return response; }
                    }
                }
            }
        }
        catch (FormatException error) { throw new ArgumentException($"Invalid Base64: '{cipherText}'", error); }
        catch (CryptographicException error) { throw new ArgumentException("Decryption failed. Key/IV mismatch or data corruption.", error); }
    }

    public static string Execute(string key, string iv, string cipherText) { string response = _decrypt(key, iv, cipherText); return response; }
}