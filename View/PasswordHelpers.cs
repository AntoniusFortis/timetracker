using System;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;

namespace Timetracker.View
{
    public class PasswordHelpers
    {
        public static bool SlowEquals(string a, string b)
        {
            int diff = a.Length ^ b.Length;
            for (int i = 0; i < a.Length && i < b.Length; i++)
                diff |= a[i] ^ b[i];
            return diff == 0;
        }

        public static byte[] GenerateSalt(int length)
        {
            byte[] salt = new byte[length];

            using (var rngCsp = new RNGCryptoServiceProvider())
            {
                rngCsp.GetBytes(salt);
            }

            return salt;
        }

        public static string EncryptPassword([NotNull] string pass, byte[] salt, int iterations)
        {
            var pbkdf2 = new Rfc2898DeriveBytes(pass, salt, iterations, HashAlgorithmName.SHA512);

            var key = pbkdf2.GetBytes(128);
            pbkdf2.Reset();

            var keyB64 = Convert.ToBase64String(key);

            return keyB64;
        }
    }
}
