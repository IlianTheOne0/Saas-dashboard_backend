namespace Base.Services.SSecurity;

using System.Security.Cryptography;
using System.Text;

internal static class Encryptor
{
    private static string _encrypt(string key, string iv, string plainText)
    {
        if (key == null || iv == null || string.IsNullOrEmpty(plainText)) { throw new ArgumentNullException("Arguments for encryption cannot be null"); }

        using (Aes aes = Aes.Create())
        {
            aes.Key = Encoding.UTF8.GetBytes(key);
            aes.IV = Encoding.UTF8.GetBytes(iv);

            ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

            using (MemoryStream ms = new MemoryStream())
            {
                using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                {
                    using (StreamWriter sw = new StreamWriter(cs)) { sw.Write(plainText); }
                    return Convert.ToBase64String(ms.ToArray());
                }
            }
        }
    }

    public static string Execute(string key, string iv, string plainText) => _encrypt(key, iv, plainText);
}