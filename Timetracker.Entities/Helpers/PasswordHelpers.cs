﻿using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Security.Cryptography;
using Timetracker.Models.Classes;

namespace Timetracker.Models.Helpers
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

        public static string GetIV()
        {
            using ( RijndaelManaged myRijndael = new RijndaelManaged() )
            {
                myRijndael.GenerateIV();

                return Convert.ToBase64String( myRijndael.IV );
            }
        }

        public static string EncryptData( [NotNull] string data, [NotNull] string IV )
        {
            var key = Convert.FromBase64String( TimetrackerAuthorizationOptions.KEYCrypt );
            var IVByte = Convert.FromBase64String( IV );

            var encrypted = Convert.ToBase64String( EncryptStringToBytes(data, key, IVByte) );

            return encrypted;
        }

        public static string DecryptData( [NotNull] string data, [NotNull] string IV )
        {
            var keyByte = Convert.FromBase64String( TimetrackerAuthorizationOptions.KEYCrypt );
            var IVByte = Convert.FromBase64String( IV );
            var dataByte = Convert.FromBase64String( data );

            return DecryptStringFromBytes( dataByte, keyByte, IVByte );
        }

        private static byte[] EncryptStringToBytes( string plainText, byte[] Key, byte[] IV )
        {
            byte[] encrypted;

            using ( var aesAlg = new RijndaelManaged() )
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;

                // Экземпляр шифратора
                var encryptor = aesAlg.CreateEncryptor( aesAlg.Key, aesAlg.IV );

                using var msEncrypt = new MemoryStream();
                using var csEncrypt = new CryptoStream( msEncrypt, encryptor, CryptoStreamMode.Write );
                using ( var swEncrypt = new StreamWriter( csEncrypt ) )
                {
                    swEncrypt.Write( plainText );
                }

                encrypted = msEncrypt.ToArray();
            }

            return encrypted;
        }

        private static string DecryptStringFromBytes( byte[] cipherText, byte[] Key, byte[] IV )
        {
            string plaintext = null;

            using ( RijndaelManaged rijAlg = new RijndaelManaged() )
            {
                rijAlg.Key = Key;
                rijAlg.IV = IV;

                // Экземпляр дешифратора
                var decryptor = rijAlg.CreateDecryptor(rijAlg.Key, rijAlg.IV);

                using MemoryStream msDecrypt = new MemoryStream( cipherText );
                using CryptoStream csDecrypt = new CryptoStream( msDecrypt, decryptor, CryptoStreamMode.Read );
                using StreamReader srDecrypt = new StreamReader( csDecrypt );
                plaintext = srDecrypt.ReadToEnd();
            }

            return plaintext;
        }
    }
}
