using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Veneka.Module.TranzwareCompassPlusFIMI.Cryptography
{
    public sealed class TripleDes
    {
        #region Constants
        private const string CHECK_VALUE_ZEROS = "0000000000000000";
        private static readonly byte[] NULL_IV = new byte[8] { 0, 0, 0, 0, 0, 0, 0, 0 };
        #endregion

        #region 3DES Encryption
        /// <summary>
        /// Encrypts data using 3DES using the supplied key
        /// </summary>
        /// <param name="key">Key used to encrypt the data, must be Hexidecimal with a length of 16, 32 or 48.</param>
        /// <param name="data">data to be encrypted, must be Hexidecimal with a length in the multiples of 16/</param>
        /// <param name="checkValue">Check value returned from encrypting the data.</param>
        /// <returns>The encrypted data in hexidecimal</returns>
        public static string EncryptTripleDES(string key, string data, out string checkValue)
        {
            string result = String.Empty;
            checkValue = String.Empty;

            if (!Utility.IsHex(key))
                throw new ArgumentException("Key is not in hexidecimal format");

            if (!Utility.IsHex(data))
                throw new ArgumentException("Data is not in hexidecimal format");

            //encrypt each 16 character block of data
            for (int i = 0; i < data.Length; i += 16)
            {
                if (data.Length < (i + 16))
                    throw new ArgumentException("Data size must be in multiples of 16.");

                string dataBlock = data.Substring(i, 16);

                result += DoEncryptTripleDES(SplitKey(key), dataBlock);
            }

            string checkValueKey = data.Length > 48 ? data.Substring(0, 48) : data;

            //Only return first 6 characters for check value
            checkValue = DoEncryptTripleDES(SplitKey(checkValueKey), CHECK_VALUE_ZEROS).Substring(0, 6);

            return result;
        }

        /// <summary>
        /// Method which encrypts a block of data using 3DES
        /// </summary>
        /// <param name="keys"></param>
        /// <param name="cipherString"></param>
        /// <returns></returns>
        private static string DoEncryptTripleDES(Tuple<string, string, string> keys, string cipherString)
        {
            string result = Encrypt(keys.Item1, cipherString);
            result = Decrypt(keys.Item2, result);
            result = Encrypt(keys.Item3, result);

            return result;
        }        
        #endregion

        #region Decryption
        /// <summary>
        /// Decrypts the data with the provided key
        /// </summary>
        /// <param name="key">Key used to encrypt the data, must be Hexidecimal with a length of 16, 32 or 48.</param>
        /// <param name="data">data to be encrypted, must be Hexidecimal with a length in the multiples of 16/</param>
        /// <returns></returns>
        public static string DecryptTripleDES(string key, string data)
        {
            string result = String.Empty;

            if (!Utility.IsHex(key))
                throw new ArgumentException("Key is not in hexidecimal format");

            if (!Utility.IsHex(data))
                throw new ArgumentException("Data is not in hexidecimal format");

            //decrypts 16 character blocks of data
            for (int i = 0; i < data.Length; i += 16)
            {
                //string dataBlock = String.Empty;

                //if (data.Length < (i + 16))
                //    dataBlock = data.Substring(i);
                //else
                //    dataBlock = data.Substring(i, 16);

                if (data.Length < (i + 16))
                    throw new ArgumentException("Data size must be in multiples of 16.");

                string dataBlock = data.Substring(i, 16);

                result += DoDecryptTripleDES(SplitKey(key), dataBlock);
            }

            return result;
        }

        /// <summary>
        /// Method which decrypts a block of data using 3DES
        /// </summary>
        /// <param name="key1">16 Characters long (Hex ASCII)</param>
        /// <param name="key2">16 Characters long (Hex ASCII)</param>
        /// <param name="key3">16 Characters long (Hex ASCII)</param>
        /// <param name="cipherString">16 Characters long (Hex ASCII)</param>
        /// <returns></returns>
        private static string DoDecryptTripleDES(Tuple<string, string, string> keys, string cipherString)
        {
            string result = Decrypt(keys.Item3, cipherString);
            result = Encrypt(keys.Item2, result);
            result = Decrypt(keys.Item1, result);

            return result;
        }        
        #endregion

        #region DES Functions
        public static string Encrypt(string key, string cipherString)
        {
            byte[] toEncryptArray = new byte[8];
            HexStringToByteArray(cipherString, toEncryptArray);

            byte[] keyArray = new byte[8];
            HexStringToByteArray(key, keyArray);

            DESCryptoServiceProvider tdes = new DESCryptoServiceProvider();
            //set the secret key for the tripleDES algorithm
            tdes.Key = keyArray;
            tdes.Mode = CipherMode.ECB;
            tdes.Padding = PaddingMode.None;
            tdes.IV = NULL_IV;

            ICryptoTransform cTransform = tdes.CreateEncryptor();
            byte[] resultArray = cTransform.TransformFinalBlock
                    (toEncryptArray, 0, toEncryptArray.Length);
            //Release resources held by TripleDes Encryptor
            tdes.Clear();
            //return the Clear decrypted TEXT
            return ByteArrayToHexString(resultArray);
        }

        public static string Decrypt(string key, string cipherString)
        {
            byte[] toEncryptArray = new byte[8];
            HexStringToByteArray(cipherString, toEncryptArray);

            byte[] keyArray = new byte[8];
            HexStringToByteArray(key, keyArray);

            DESCryptoServiceProvider tdes = new DESCryptoServiceProvider();
            //set the secret key for the tripleDES algorithm
            tdes.Key = keyArray;
            tdes.Mode = CipherMode.ECB;
            tdes.Padding = PaddingMode.None;
            tdes.IV = NULL_IV;

            ICryptoTransform cTransform = tdes.CreateDecryptor();
            byte[] resultArray = cTransform.TransformFinalBlock
                    (toEncryptArray, 0, toEncryptArray.Length);
            //Release resources held by TripleDes Encryptor
            tdes.Clear();
            //return the Clear decrypted TEXT
            return ByteArrayToHexString(resultArray);
        }
        #endregion

        #region Helpers
        private static Tuple<string, string, string> SplitKey(string key)
        {
            if (key.Length > 48)
                throw new ArgumentException("Key length is to large, must be 42 characters or shorter in multiples of 16.");

            string[] keys = new string[3];

            for (int i = 0; i < key.Length; i += 16)
            {
                if (key.Length < (i + 16))
                    throw new ArgumentException("Key size must be in multiples of 16.");

                keys[i / 16] = key.Substring(i, 16);
            }

            if (keys[1] == null && keys[2] == null) //Set all the keys the same
                keys[1] = keys[2] = keys[0];
            else if (keys[2] == null) //Set the first and last key as the same.
                keys[2] = keys[0];

            return new Tuple<string, string, string>(keys[0], keys[1], keys[2]);
        }

        private static void HexStringToByteArray(string s, byte[] bData)
        {
            int i = 0;
            int j = 0;

            while (i <= s.Length - 1)
            {
                bData[j] = Convert.ToByte(s.Substring(i, 2), 16);
                i += 2;
                j += 1;
            }
        }

        private static string ByteArrayToHexString(byte[] bData)
        {
            StringBuilder sb = new StringBuilder();

            foreach (byte b in bData)
            {
                sb.AppendFormat("{0:X2}", b);
            }

            return sb.ToString();
        }
        #endregion
    }
}
