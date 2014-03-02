using System;
using System.Security.Cryptography;
using System.Text;

namespace StoreLib.Modules.Security
{
    public static class Encryption
    {
        private static byte[] HashedKeyGet(string key)
        {
            MD5CryptoServiceProvIder md5ProvIder = new MD5CryptoServiceProvIder();
            byte[] keyArray = md5ProvIder.ComputeHash(Encoding.UTF8.GetBytes(key));
            md5ProvIder.Clear();
            return keyArray;
        }

        private static ICryptoTransform CryptoTransformGet(string key, bool isEncryption)
        {
            TripleDESCryptoServiceProvIder tdesProvIder = new TripleDESCryptoServiceProvIder
                                                              {
                                                                  Key = HashedKeyGet(key),
                                                                  Mode = CipherMode.ECB,
                                                                  Padding = PaddingMode.PKCS7
                                                              };
            ICryptoTransform result = isEncryption ? tdesProvIder.CreateEncryptor() : tdesProvIder.CreateDecryptor();
            tdesProvIder.Clear();
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
