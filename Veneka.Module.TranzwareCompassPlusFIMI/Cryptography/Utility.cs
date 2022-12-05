using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Veneka.Module.TranzwareCompassPlusFIMI.Cryptography
{
    public sealed class Utility
    {
        public enum ParityCheck
        {
            OddParity = 0,
            EvenParity = 1,
            NoParity = 2
        }

        public static string MakeParity(string hexString, ParityCheck parity)
        {
            if (parity == ParityCheck.NoParity)
                return hexString;

            string head = "";
            //if (hexString != RemoveKeyType(hexString))
            //{
            //    head = hexString.Substring(0, 1);
            //    hexString = RemoveKeyType(hexString);
            //}

            int i = 0;
            string r = "";

            while (i < hexString.Length)
            {
                string b = toBinary(hexString.Substring(i, 2));
                i += 2;
                int l = b.Replace("0", "").Length;

                if (((l % 2 == 0) && (parity == ParityCheck.OddParity)) || ((l % 2 == 1) && (parity == ParityCheck.EvenParity)))
                {
                    if (b.Substring(7, 1) == "1")
                    {
                        r = r + b.Substring(0, 7) + "0";
                    }
                    else
                    {
                        r = r + b.Substring(0, 7) + "1";
                    }
                }
                else
                {
                    r = r + b;
                }
            }

            return head + fromBinary(r);
        }

        /// <summary>
        /// XOR All strings in an array
        /// </summary>
        /// <param name="keys"></param>
        /// <returns></returns>
        public static string XORAllKeys(string[] keys)
        {
            string xorred = keys[0];
            for (int i = 1; i <= keys.GetUpperBound(0); i++)
            {
                xorred = Utility.XORHexStringsFull(xorred, keys[i]);
            }
            return xorred;
        }


        /// <summary>
        /// XOR Two strings.
        /// </summary>
        /// <param name="s1"></param>
        /// <param name="s2"></param>
        /// <returns></returns>
        public static string XORHexStringsFull(string s1, string s2)
        {
            string s = "";

            for (int i = 0; i <= s1.Length - 1; i++)
            {
                s = s + (Convert.ToInt32(s1.Substring(i, 1), 16) ^ Convert.ToInt32(s2.Substring(i, 1), 16)).ToString("X");
            }

            return s;
        }

        /// <summary>
        /// Hex to Binary
        /// </summary>
        /// <param name="hexString"></param>
        /// <returns></returns>
        public static string toBinary(string hexString)
        {
            string r = "";
            for (int i = 0; i <= hexString.Length - 1; i++)
            {
                r = r + Convert.ToString(Convert.ToInt32(hexString.Substring(i, 1), 16), 2).PadLeft(4, '0');
            }
            return r;
        }


        /// <summary>
        /// Binary to Hex
        /// </summary>
        /// <param name="binaryString"></param>
        /// <returns></returns>
        public static string fromBinary(string binaryString)
        {
            string r = "";
            for (int i = 0; i <= binaryString.Length - 1; i += 4)
            {
                r = r + Convert.ToByte(binaryString.Substring(i, 4), 2).ToString("X1");
            }
            return r;
        }

        /// <summary>
        /// Check is a string value is hexidecimal
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsHex(string value)
        {
            bool isHex;
            foreach (var c in value)
            {
                isHex = ((c >= '0' && c <= '9') ||
                         (c >= 'a' && c <= 'f') ||
                         (c >= 'A' && c <= 'F'));

                if (!isHex)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// String to hexidecimal
        /// </summary>
        /// <param name="asciiString"></param>
        /// <returns></returns>
        public static string StringToHex(string asciiString)
        {
            string hex = "";
            foreach (char c in asciiString)
            {
                int tmp = c;
                hex += String.Format("{0:x2}", (uint)System.Convert.ToUInt32(tmp.ToString()));
            }
            return hex;
        }

        /// <summary>
        /// Hexidecimal to string.
        /// </summary>
        /// <param name="HexValue"></param>
        /// <returns></returns>
        public static string HexToString(string HexValue)
        {
            string StrValue = "";
            while (HexValue.Length > 0)
            {
                StrValue += System.Convert.ToChar(System.Convert.ToUInt32(HexValue.Substring(0, 2), 16)).ToString();
                HexValue = HexValue.Substring(2, HexValue.Length - 2);
            }
            return StrValue;
        }
    }
}
