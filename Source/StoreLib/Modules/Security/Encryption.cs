using System;
using System.Security.Cryptography;
using System.Text;

namespace StoreLib.Modules.Security
{
    public static class Encryption
    {
        private static byte[] HashedKeyGet(string key)
        {
            MD5CryptoServiceProvider md5Provider = new MD5CryptoServiceProvider();
            byte[] keyArray = md5Provider.ComputeHash(Encoding.UTF8.GetBytes(key));
            md5Provider.Clear();
            return keyArray;
        }

        private static ICryptoTransform CryptoTransformGet(string key, bool isEncryption)
        {
            TripleDESCryptoServiceProvider tdesProvider = new TripleDESCryptoServiceProvider
                                                              {
                                                                  Key = HashedKeyGet(key),
                                                                  Mode = CipherMode.ECB,
                                                                  Padding = PaddingMode.PKCS7
                                                              };
            ICryptoTransform result = isEncryption ? tdesProvider.CreateEncryptor() : tdesProvider.CreateDecryptor();
            tdesProvider.Clear();
            return result;
        }

        public static string PasswordEncrypt(string password, string key)
        {
            byte[] bArray = Encoding.UTF8.GetBytes(password);
            ICryptoTransform cryptoTransform = CryptoTransformGet(key, true);
            bArray = cryptoTransform.TransformFinalBlock(bArray, 0, bArray.Length);
            return Convert.ToBase64String(bArray, 0, bArray.Length);
        }

        public static string PasswordDecrypt(string password, string key)
        {
            byte[] bArray = Encoding.UTF8.GetBytes(password);
            ICryptoTransform cryptoTransform = CryptoTransformGet(key, false);
            bArray = cryptoTransform.TransformFinalBlock(bArray, 0, bArray.Length);
            return Convert.ToBase64String(bArray, 0, bArray.Length);
        }
    }
}
