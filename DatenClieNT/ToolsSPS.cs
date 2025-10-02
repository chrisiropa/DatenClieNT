using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;

namespace DatenClieNT
{
   class ToolsSPS
   {
      public static string PcFloatToS5Hex(double ff)
      {
         Boolean sign = false;
         byte kgExponent = 23;
         
         if(ff < 0.0)
         {
            sign = true;
            ff = Math.Abs(ff);
         }
         

         int f2 = (int)ff;

         if (ff == 0.0)
         {
            return "80000000";
         }

         while (f2 > 0x00400000)
         {
            ff /= 2;
            f2 = (int)ff;
            kgExponent++;
         }

         while (f2 < 0x00400000)
         {
            ff *= 2;
            f2 = (int)ff;
            kgExponent--;
         }
         
         if(sign)
         {
            f2 = -f2;            
            f2 &= 0x00FFFFFF;
         }

         string hex = string.Format("{0:X2}{1:X6}", kgExponent, f2);

         return hex;
      }

      public static float S5HexToPcFloat(string hex)
      {
         byte[] hexBytes = new byte[4];
         byte[] workBytes = new byte[4];
         
         if (hex.Length != 8)
         {
            return float.NaN;
         }

         hexBytes[0] = Convert.ToByte(hex.Substring(0, 2), 16);
         hexBytes[1] = Convert.ToByte(hex.Substring(2, 2), 16);
         hexBytes[2] = Convert.ToByte(hex.Substring(4, 2), 16);
         hexBytes[3] = Convert.ToByte(hex.Substring(6, 2), 16);
         
         Int32 kgExponent = (Int32) hexBytes[0];
         
         if(kgExponent > 0x7F)
         {
            kgExponent = kgExponent - 0x100;
         }

         workBytes[3] = 0;
         workBytes[2] = hexBytes[1];
         workBytes[1] = hexBytes[2];
         workBytes[0] = hexBytes[3];

         Int32 sign = (workBytes[2] & ((Int32)0x80));
         workBytes[2] &= 0x7F;

         Int32 mantisse = 0;

         mantisse += (Int32)(workBytes[3] << 24);
         mantisse += (Int32)(workBytes[2] << 16);
         mantisse += (Int32)(workBytes[1] << 8);
         mantisse += (Int32)(workBytes[0]);

         if (sign > 0)
         {
            unchecked
            {
               mantisse = mantisse ^ ((Int32)0xFFFFFFFF);
               mantisse = mantisse + 0x00800000;
            }            
            
            mantisse = - mantisse;
         }

         double value = mantisse;
         
         
         /*
         while (kgExponent > 23)
         {
            value *= 2;
            kgExponent--;
         }

         
         while (kgExponent < 23)
         {
            value /= 2;
            kgExponent++;
         }
         */
         
         if(kgExponent < 23)
         {
            value /= (double)Math.Pow(2.0, ((double)23.0) - ((double)kgExponent));
         }
         else if (kgExponent > 23)
         {
            value *= (double)Math.Pow(2.0, (((double)kgExponent) - (double)23.0));
         }
         
         return (float)value;
      }
   }
}
