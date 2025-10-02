using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;

namespace DatenClieNT
{
   public class EndianConversion
   {
      private byte[] source;

      private void SetSource(object value)
      {
         source = new byte[Marshal.SizeOf(value)];

         IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf(value));

         try
         {
            Marshal.StructureToPtr(value, ptr, true);
            Marshal.Copy(ptr, source, 0, Marshal.SizeOf(value));
         }
         finally
         {
            Marshal.FreeHGlobal(ptr);
         }
      }

      private void SetSource(byte[] value, int startByte, int len)
      {
         source = new byte[len];

         MemoryStream stream = new MemoryStream(value);

         stream.Read(source, startByte, len);
      }

      public EndianConversion(UInt16 value)
      {
         SetSource(value);
      }
      public EndianConversion(UInt32 value)
      {
         SetSource(value);
      }
      public EndianConversion(float value)
      {
         SetSource(value);
      }

      public EndianConversion(byte[] value, int startByte, int len)
      {
         SetSource(value, startByte, len);
      }

      public byte[] GetBuffer()
      {
         byte[] destination = new byte[source.Length];


         //return Array.Reverse(source); // now is in little endian
         
         
         

         //Bytes umgedreht zurückgeben
         for (int i = 0; i < source.Length; i++)
         {
            destination[i] = source[source.Length - 1 - i];
         }

         return destination;
      }

      public Int64 GetInt64(double faktor, double offset)
      {
         Int64 i64Value = 0;
         

         try
         {
            switch (GetBuffer().Length)
            {
               case 1:
                  i64Value = (Int64)BitConverter.ToChar(GetBuffer(), 0);
                  break;
               case 2:
                  i64Value = (Int64)BitConverter.ToInt16(GetBuffer(), 0);
                  break;
               case 4:
                  i64Value = (Int64) BitConverter.ToInt32(GetBuffer(), 0);
                  break;
               case 8:
                  i64Value = (Int64)BitConverter.ToInt64(GetBuffer(), 0);
               break;
            }
         }
         catch
         {
         }

         if (faktor != double.NaN && faktor != 0)
         {
            i64Value /= ((Int64) faktor);
         }

         if (offset != double.NaN)
         {
            i64Value += ((Int64)offset);
         }

         return i64Value;
      }
   }
}
