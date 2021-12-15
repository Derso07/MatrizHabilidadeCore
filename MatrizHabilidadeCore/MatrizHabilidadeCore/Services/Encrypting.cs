using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MatrizHabilidadeDataBaseCore.Services
{
    public class Encrypting
    {
            private static byte[] IV { get { return StringToByte("5a992e962d9395322d21163f966cf19561023938d3c91ae073b6ddc38cb8dccace2e72ae9257d25f6a64bea2315e755af83e3be16d6c8e0344eee8f5cbbefa7c"); } }
            private static byte[] Key { get { return StringToByte("576f685221305de93dfbee38f38830089a77392a5ddd06cfb6a060835dbb9df378ef5a7f07f7a3b6ad3170f78ef7158e40d383004de5b5d7a920cf37c0c41a31"); } }

            private static byte[] StringToByte(string stringToConvert)
            {
                byte[] key = new byte[16];
                for (int i = 0; i < 16; i += 2)
                {
                    byte[] unicodeBytes = BitConverter.GetBytes(stringToConvert[i % stringToConvert.Length]);
                    Array.Copy(unicodeBytes, 0, key, i, 2);
                }
                return key;
            }

            public static string Encrypt(string plainText)
            {
                byte[] encrypted;

                using (Aes aesAlg = Aes.Create())
                {
                    aesAlg.Key = Key;
                    aesAlg.IV = IV;

                    ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                    using (MemoryStream msEncrypt = new MemoryStream())
                    {
                        using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                        {
                            using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                            {
                                swEncrypt.Write(plainText);
                            }

                            encrypted = msEncrypt.ToArray();
                        }
                    }
                }

                return Convert.ToBase64String(encrypted).Replace('+', '-').Replace('/', '_');
            }

            public static string Decrypt(string cipherText)
            {
                string plaintext = null;

                if (string.IsNullOrEmpty(cipherText))
                    return "";

                cipherText = cipherText.Replace('-', '+').Replace('_', '/');

                try
                {
                    using (Aes aesAlg = Aes.Create())
                    {
                        aesAlg.Key = Key;
                        aesAlg.IV = IV;

                        ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                        using (MemoryStream msDecrypt = new MemoryStream(Convert.FromBase64String(cipherText)))
                        using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                            plaintext = srDecrypt.ReadToEnd();
                    }
                }
                catch { }

                return plaintext;
            }
        }
    }
