using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;

namespace DatenClieNT
{
   class StoerBerNurAnzeige
   {
      Dictionary<int, int> nurAnzeige = new Dictionary<int, int>();

      public StoerBerNurAnzeige(string iniString)
      {
         try
         {
            iniString = ";" + iniString;
            iniString = iniString.Replace(" ", "");
            iniString = iniString.Replace(";", ",");
            iniString = iniString.Replace(":", ",");
            iniString = iniString.Replace("-", ",");
            iniString = iniString.Replace("#", ",");

            string[] nurAnzBereiche = iniString.Split(',');

            foreach (string bereich in nurAnzBereiche)
            {
               if (bereich.Length < 1)
               {
                  continue;
               }

               int key;
               int value;

               key = value = Convert.ToInt32(bereich);
               nurAnzeige[key] = value;
            }
         }
         catch (Exception e)
         {
            LogManager.GetSingleton().ZLog("C0246", ELF.ERROR, "Feld -> StoerBerNurAnz in der Tabelle DC_Datenclients konnte nicht ausgewertet werden. Daher wird angenommen, das alle Störungen in die Datenbank eingetragen werden ! -> {0}", e.Message);
            LogManager.GetSingleton().ZLog("C0247", ELF.INFO, "Feld -> StoerBerNurAnz in der Tabelle DC_Datenclients muß z.B. so angegeben werden -> '1,4,10,15' (Ohne Hochkommas)");
         }
      }

      public Boolean StoerNurAnzeige(int bereich)
      {
         return nurAnzeige.ContainsKey(bereich);
      }
   }
}
