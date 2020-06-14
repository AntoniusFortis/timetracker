using System;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;

namespace Timetracker.Entities
{
    public static class PasswordHelpers
    {
        private const int SaltSize = 16;
        private const int IterationsCount = 1024;

        public static bool SlowEquals( [NotNull] string a, [NotNull] string b)
        {
            if ( a.Length != b.Length )
            {
                return false;
            }

            int diff = a.Length ^ b.Length;
            for (int i = 0; i < a.Length && i < b.Length; i++)
                diff |= a[i] ^ b[i];
            return diff == 0;
        }

        public static byte[] GenerateSalt()
        {
            var salt = new byte[SaltSize];

            using (var rngCsp = new RNGCryptoServiceProvider())
            {
                rngCsp.GetBytes(salt);
            }

            return salt;
        }

        public static string EncryptPassword( [NotNull] string pass, [NotNull] byte[] salt )
        {
            var pbkdf2 = new Rfc2898DeriveBytes(pass, salt, IterationsCount, HashAlgorithmName.SHA512);

            var key = pbkdf2.GetBytes(128);
            pbkdf2.Reset();

            var keyB64 = Convert.ToBase64String(key);

            return keyB64;
        }
    }
}
