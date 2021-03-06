﻿using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Marchive.App.Exceptions;

namespace Marchive.App.Services
{
    // https://docs.microsoft.com/en-us/dotnet/api/system.security.cryptography.aes?view=netcore-3.1
    // https://docs.microsoft.com/en-us/dotnet/api/system.security.cryptography.rfc2898derivebytes?view=netcore-3.1

    /// <summary>
    /// Provides data confidentiality and integrity using AES cryptography and HMAC message authentication
    /// 
    /// Encryption data format
    /// +------------------------------------------------------+
    /// | SALT | INITIALIZATION VECTOR | ENCRYPTED DATA | HMAC |
    /// +------------------------------------------------------+
    /// The data is encrypted using AES with a 128 bit key derived from the provided password
    /// The salt and initialization vectors are created in a transient manner and stored in the output data
    /// A HMAC (SHA256) is computed from the above data using the same key and appended to the final output data
    /// </summary>
    internal class AesEncryption : IEncryptionAlgorithm
    {
        private const int KeyDerivationIterations = 50000;
        private const int KeySizeBytes = 16; // (128 bits)
        private const int SaltSizeBytes = 8;  // (64 bits)
        private const int InitializationVectorSizeBytes = 16; // (128 bits)
        private const int HMacSizeBytes = 32; // (256 bits)

        public byte[] Encrypt(byte[] data, string password)
        {
            if (data == null || data.Length <= 0)
                throw new EncryptionException("No data to encrypt.");
            if (string.IsNullOrWhiteSpace(password))
                throw new EncryptionException("Empty password provided for encryption.");

            var initializationVector = CreateInitializationVector();
            using var aes = Aes.Create();
            if (aes == null)
            {
                throw new EncryptionException("Could not create an AES crypto service provider.");
            }

            var salt = CreateSalt();
            aes.Key = DeriveKey(password, salt);
            aes.IV = initializationVector;

            var encryptor = aes.CreateEncryptor();

            using var encryptionStream = new MemoryStream();

            // Write salt and IV to encryptionStream
            encryptionStream.Write(salt, 0, salt.Length);
            encryptionStream.Write(initializationVector, 0, initializationVector.Length);

            // Encrypt data and write it to encryptionStream
            using var cryptoStream = new CryptoStream(encryptionStream, encryptor, CryptoStreamMode.Write);
            cryptoStream.Write(data, 0, data.Length);

            cryptoStream.FlushFinalBlock();
            cryptoStream.Close();

            // Compute HMAC
            var encryptedData = encryptionStream.ToArray();
            var hMac = ComputeHmac256(aes.Key, encryptedData);

            // Merge encrypted data and HMAC into final payload
            var finalData = new byte[encryptedData.Length + hMac.Length];
            encryptedData.CopyTo(finalData, 0);
            hMac.CopyTo(finalData, encryptedData.Length);

            return finalData;
        }

        private static byte[] DeriveKey(string password, byte[] salt)
            => new Rfc2898DeriveBytes(password, salt, KeyDerivationIterations,
                HashAlgorithmName.SHA512).GetBytes(KeySizeBytes);

        private static byte[] CreateSalt()
        {
            var salt = new byte[SaltSizeBytes];
            using var rngCsp = new RNGCryptoServiceProvider();
            rngCsp.GetBytes(salt);
            return salt;
        }

        private static byte[] CreateInitializationVector()
            => GetRandomData(InitializationVectorSizeBytes);

        public byte[] Decrypt(byte[] data, string password)
        {
            if (data == null || data.Length <= 0)
                throw new EncryptionException("No data to decrypt.");
            if (string.IsNullOrWhiteSpace(password))
                throw new InvalidEncryptionKeyException();

            // Get salt and IV from data
            var salt = new byte[SaltSizeBytes];
            Array.Copy(data, salt, SaltSizeBytes);
            var initializationVector = new byte[InitializationVectorSizeBytes];
            Array.Copy(data, SaltSizeBytes, initializationVector, 0, InitializationVectorSizeBytes);

            using var aes = Aes.Create();
            if (aes == null)
            {
                throw new EncryptionException("Could not create an AES crypto service provider.");
            }
            aes.Key = DeriveKey(password, salt);
            aes.IV = initializationVector;

            // Validate integrity
            var encryptedData = new byte[data.Length - HMacSizeBytes];
            var hMac = new byte[HMacSizeBytes];
            Array.Copy(data, encryptedData, data.Length - HMacSizeBytes);
            Array.Copy(data, data.Length - HMacSizeBytes, hMac, 0, HMacSizeBytes);
            var computedHMac = ComputeHmac256(aes.Key, encryptedData);
            if (!hMac.SequenceEqual(computedHMac))
            {
                throw new InvalidEncryptionKeyException();
            }

            var decryptor = aes.CreateDecryptor();

            using var decryptionStream = new MemoryStream();
            using var cryptoStream = new CryptoStream(decryptionStream, decryptor, CryptoStreamMode.Write);

            cryptoStream.Write(encryptedData, SaltSizeBytes + InitializationVectorSizeBytes, encryptedData.Length - SaltSizeBytes - InitializationVectorSizeBytes);

            cryptoStream.FlushFinalBlock();
            cryptoStream.Close();

            return decryptionStream.ToArray();
        }

        private static byte[] GetRandomData(int length)
        {
            var randomData = new byte[length];
            RandomNumberGenerator.Fill(randomData);
            return randomData;
        }

        private byte[] ComputeHmac256(byte[] key, byte[] data)
        {
            using var hmac = new HMACSHA256(key);
            return hmac.ComputeHash(data);
        }
    }
}