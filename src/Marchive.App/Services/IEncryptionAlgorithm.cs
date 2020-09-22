namespace Marchive.App.Services
{
    internal interface IEncryptionAlgorithm
    {
        byte[] Encrypt(byte[] data, string password);
        byte[] Decrypt(byte[] data, string password);
    }
}