using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatenClieNT
{
	public static class SimplePositionalCoder
   {
       // Encode: fügt auf das n-te Byte den Wert (n+1) hinzu (mod 256) und liefert Base64-String.
       public static string Encode(string plain)
       {
           if (plain == null) throw new ArgumentNullException(nameof(plain));
           var bytes = Encoding.UTF8.GetBytes(plain);
           var outb = new byte[bytes.Length];
           for (int i = 0; i < bytes.Length; i++)
           {
               // add i+1 to byte, wrap modulo 256
               outb[i] = (byte)((bytes[i] + (i + 1)) & 0xFF);
           }
           return Convert.ToBase64String(outb);
       }

       // Decode: nimmt Base64-String, zieht (n+1) vom n-ten Byte ab (mod 256) und gibt UTF8-String zurück.
       public static string Decode(string base64)
       {
           if (base64 == null) throw new ArgumentNullException(nameof(base64));
           var inb = Convert.FromBase64String(base64);
           var outb = new byte[inb.Length];
           for (int i = 0; i < inb.Length; i++)
           {
               // subtract i+1 (add 256 first to avoid negative, then mod 256)
               outb[i] = (byte)((inb[i] - (i + 1)) & 0xFF);
           }
           return Encoding.UTF8.GetString(outb);
       }
   }
}
