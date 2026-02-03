using System;
using System.Text;

namespace DatenClieNT
{
   public static class CryptoHelper
   {
       private static readonly string key = "apori72"; // dein Schlüssel

       public static string Encrypt(string plainText)
      {
         // XOR
         byte[] data = Encoding.UTF8.GetBytes(plainText);
         for (int i = 0; i < data.Length; i++)
         {
            data[i] = (byte)(data[i] ^ (byte)key[i % key.Length]);
         }

         // In HEX umwandeln
         StringBuilder sb = new StringBuilder();
         foreach (byte b in data)
            sb.AppendFormat("{0:X2}", b);

         return sb.ToString();
      }

      public static string Decrypt(string hexText)
      {
         try
         {
            // Hex zurück in Bytes
            int len = hexText.Length / 2;
            byte[] data = new byte[len];
            for (int i = 0; i < len; i++)
            {
               data[i] = Convert.ToByte(hexText.Substring(i * 2, 2), 16);
            }

            // XOR zurück
            for (int i = 0; i < data.Length; i++)
            {
               data[i] = (byte)(data[i] ^ (byte)key[i % key.Length]);
            }

            return Encoding.UTF8.GetString(data);
         }
         catch (Exception e)
         {
            
         }

         return "";

      }
   }
}
