using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System;
using System.IO;


namespace DatenClieNT
{
	
   public enum FailReason
   {
      OK = 0,
      MARKER_NOT_FOUND = 1,
      HASH_INVALID = 2,
      ERROR_MESSAGE = 3,
      UNKNOWN = 10
   }

   public static class EmbeddedSignature
   {         
      public static int MarkerLen = 20;
      public static readonly byte[] MarkerAndPayload = new byte[52] 
      //public static readonly byte[] Marker = new byte[20] 
      {
         //Markerbereich Start               
         //Die ersten 20 Nachkommastellen von PI sind der Marker
            (byte)'1',(byte)'4',(byte)'1',(byte)'5',(byte)'9',(byte)'2',(byte)'6',(byte)'5',(byte)'3',(byte)'5',
            (byte)'8',(byte)'9',(byte)'7',(byte)'9',(byte)'3',(byte)'2',(byte)'3',(byte)'8',(byte)'4',(byte)'6'
         //Markerbereich Ende
         //Payloadbereich Start. Ab hier fügt der Signer den Hash ein (32 Byte) 
         //Der Payload-Bereich muß später für die Hash-Berechnung wieder so manipuliert werden.
         //Er wurde ja durch den Signer auf SHA256 durch 32 gültige Bytes beschrieben. 
          ,0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
      };

      public const int MarkerLength = 20;
      public const int PayloadLength = 32;

      // Sichtbare Teilarrays für den Code:
      public static byte[] Marker => MarkerAndPayload.Take(MarkerLength).ToArray();
      public static int PayloadOffset => MarkerLength;
   }
   
   public class SelfVerifier
   {
       private int IndexOfKMP(byte[] haystack, byte[] needle)
      {
            if (needle == null || needle.Length == 0) return 0;
            if (haystack == null || haystack.Length < needle.Length) return -1;

            int[] lps = BuildLps(needle);
            int i = 0, j = 0;
            while (i < haystack.Length)
            {
               if (haystack[i] == needle[j]) { i++; j++; if (j == needle.Length) return i - j; }
               else if (j > 0) j = lps[j - 1];
               else i++;
            }
            return -1;
      }

      private int[] BuildLps(byte[] pat)
      {
            int m = pat.Length;
            int[] lps = new int[m];
            lps[0] = 0;
            int len = 0, i = 1;
            while (i < m)
            {
               if (pat[i] == pat[len]) { len++; lps[i] = len; i++; }
               else if (len != 0) len = lps[len - 1];
               else { lps[i] = 0; i++; }
            }
            return lps;
      }



       public FailReason VerifySelf()
       {
           try
           {
               var path = Assembly.GetExecutingAssembly().Location;
               var bin = File.ReadAllBytes(path);

               //Console.WriteLine("Datei die überprüft wird = '{0}'", path);
               //LogManager.GetSingleton().ZLog("CD263", ELF.ERROR, "Error in GetTelegramItems -> {0}", e.Message);
               //Console.WriteLine("{0}", EmbeddedSignature.Marker.Length);
               //Console.WriteLine("Needle (HEX): " + BitConverter.ToString(EmbeddedSignature.Marker));

               LogManager.GetSingleton().ZLog("CD263", ELF.INFO, "Datei ({0} wird auf Manipulation gecheckt -> {0}", path);


               var idx = IndexOfKMP(bin, EmbeddedSignature.Marker);
               if (idx < 0) 
               {
                  //Console.WriteLine("MARKER_NOT_FOUND");
                  return FailReason.MARKER_NOT_FOUND;
               }
               //Console.WriteLine("MARKER FOUND");
               
               var hashBuffer = new byte[bin.Length];
               Array.Copy(bin, hashBuffer, bin.Length);
         
               int payloadOffset = idx + EmbeddedSignature.Marker.Length; 
               int payloadLength = 32; 

               //Console.WriteLine("payloadOffset = 0x{0:x}", payloadOffset);

               var payload = new byte[32]; 
               Array.Copy(bin, payloadOffset, payload, 0, payload.Length);

               //Console.WriteLine("SHA256 Gefunden:  " + BitConverter.ToString(payload).ToLowerInvariant());


               // Vor der Hash-Berechnung Payload nullen
               for (int i = 0; i < payloadLength; i++)
               {
                  hashBuffer[payloadOffset + i] = 0;
               }

               

               // Compute SHA256 over the modified buffer
               byte[] digest;
               using (var sha = SHA256.Create())
               {
                  digest = sha.ComputeHash(hashBuffer);
               }

               //Console.WriteLine("SHA256 Berechnet: " + BitConverter.ToString(digest).ToLowerInvariant());

               // Compare
               if (payload.SequenceEqual(digest))
               {
                  //Console.WriteLine("HASH VALID");   
                  return FailReason.OK;
               }
               else
               {
                  //Console.WriteLine("HASH INVALID");   
                  return FailReason.HASH_INVALID;
               }
           }
           catch(Exception e)
           {
               LogManager.GetSingleton().ZLog("CD263", ELF.ERROR, "Fehler beim checken auf Manipulation -> {0}", e.Message);
               return FailReason.ERROR_MESSAGE;
           }
       }
   }
}
