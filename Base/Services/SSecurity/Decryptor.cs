namespace Base.Services.SSecurity;

using System.Security.Cryptography;
using System.Text;

internal static class Decryptor
{
    private static string _decrypt(string key, string iv, string cipherText)
    {
        if (key == null || iv == null || string.IsNullOrEmpty(cipherText)) { throw new ArgumentNullException("Arguments for encryption cannot be null"); }

        try
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(key);
                aes.IV = Encoding.UTF8.GetBytes(iv);

                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                using (MemoryStream ms = new MemoryStream(Convert.FromBase64String(cipherText)))
                {
                    using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader sr = new StreamReader(cs)) { return sr.ReadToEnd(); }
                    }
                }
            }
        }
        catch (FormatException error) { throw new ArgumentException("The provided plainText is not a valid Base64 string", error); }
    }

    public static string Execute(string key, string iv, string cipherText) => _decrypt(key, iv, cipherText);
}