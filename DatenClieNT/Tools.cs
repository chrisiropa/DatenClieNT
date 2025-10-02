using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
using System.Runtime.InteropServices;


namespace DatenClieNT
{
   class Tools
   {
      public static string GetString(Dictionary<string, object> prm, string key)
      {
         string ret = "";
         

         try
         {
            ret = (string)prm[key];
         }
         catch
         {
            throw new Exception("Konfigurationsfehler in der Datenbank -> DC_DatenclientsTIA");
         }

         return ret;
      }

      public static string GetString(Dictionary<string, object> prm, string key, string defaultWert)
      {
         string ret = defaultWert;

         try
         {
            ret = (string)prm[key];
         }
         catch
         {
         }

         return ret;
      }
      
      public static string ImplantInto(string text, string fragment, int pos)
      {
         StringBuilder neuerText = new StringBuilder(text);
         
         for (int i = 0; i < fragment.Length; i++)
         {
            if ((pos + i) < neuerText.Length)
            {
               neuerText[pos + i] = fragment[i];
            }
            else
            {
               throw new Exception(string.Format("Tools.ImplantInto -> fragment '{0}' konnte nicht in Text '{1}' an Position {2} eingebaut werden !", text, fragment, pos));
            }
         }

         return neuerText.ToString();
      }

      public static Boolean GetBoolean(Dictionary<string, object> prm, string key)
      {
         Boolean ret = false;

         try
         {
            ret = (Boolean)prm[key];
         }
         catch
         {
            throw new Exception("Konfigurationsfehler in der Datenbank -> DC_DatenclientsTIA");
         }

         return ret;
      }


      public static long GetLong(Dictionary<string, object> prm, string key)
      {
         long ret = 0;

         try
         {
            //ret = (long)prm[key];
            ret = Convert.ToInt64(prm[key]);
         }
         catch
         {
            //Hier nicht mitloggen !
            throw new Exception("Konfigurationsfehler in der Datenbank -> DC_DatenclientsTIA");
         }

         return ret;
      }

      public static string ShortToHexString(short value)
      {
         return string.Format("{0:X04}", value);
      }
      public static string ByteToHexString(byte value)
      {
         return string.Format("{0:X02}", value);
      }

      public static byte LeftByteFromShort(short value)
      {
         string hex = ShortToHexString(value);

         hex = hex.Substring(0, 2);

         return Convert.ToByte(hex, 16);
      }

      public static byte RightByteFromShort(short value)
      {
         string hex = ShortToHexString(value);

         hex = hex.Substring(2, 2);

         return Convert.ToByte(hex, 16);
      }

      public static int SizeOfObject(object obj)
      {
         int byteLen = 0;

         try
         {
            if (obj.GetType().IsArray)
            {
               byteLen = ((Array)obj).Length;

               if (byteLen > 0)
               {
                  byteLen *= Marshal.SizeOf(((Array)obj).GetValue(0));
               }
            }
            else
            {
               switch (Type.GetTypeCode(obj.GetType()))
               {
                  case TypeCode.Int16: byteLen = Marshal.SizeOf((Int16)obj); break;
                  case TypeCode.Int32: byteLen = Marshal.SizeOf((Int32)obj); break;
                  case TypeCode.Int64: byteLen = Marshal.SizeOf((Int64)obj); break;
                  case TypeCode.Double: byteLen = Marshal.SizeOf((double)obj); break;
                  case TypeCode.Decimal: byteLen = Marshal.SizeOf((Decimal)obj); break;
                  case TypeCode.Byte: byteLen = Marshal.SizeOf((Byte)obj); break;
                  case TypeCode.Single: byteLen = Marshal.SizeOf((Single)obj); break;
                  case TypeCode.Boolean: byteLen = Marshal.SizeOf((Boolean)obj); break;
                  case TypeCode.String: byteLen = ((string)obj).Length; break;
                  case TypeCode.DateTime: byteLen = Marshal.SizeOf((DateTime)obj); break;
               }
            }
         }
         catch (Exception e)
         {
            LogManager.GetSingleton().ZLog("C0296", ELF.ERROR, "OpcServer.ObjectLen -> {0}", e.Message);
         }

         return byteLen;
      }

      public static string SaveSubstring(string text, int startPos, int len)
      {
         //Diese Funktion kapselt die .NET- Funktion Substring
         //Dabei wird weiterhin eine Exception geworfen wenn die 
         //Startposition schon größer ist als die Stringlänge.
         //Allerdings wird KEINE Exception geworfen, wenn 
         //startPos + len nicht mehr innerhalb des Srings liegt.
         //Stattdessen wird die Länge auf die zur Verfügung 
         //Restlänge begrenzt
         
         string ret = "";
         
         if(startPos < text.Length)
         {
            int saveLen = Math.Min(len, text.Length - startPos);
            
            ret = text.Substring(startPos, saveLen); 
         }
         else
         {
            throw new Exception("Tools.SaveSubstring -> Startposition groesser als Laenge des Strings !");
         }
         
         return ret;
      }
   }
}
