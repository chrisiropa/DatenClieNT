using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace DatenClieNT_AuftragsGenerator
{
   public class DatatypeRecognizer
   {
      private Boolean warning = true;
      
      public string GetSqlDatatype(string dataType)
      {
         dataType = dataType.ToUpper();

         if (dataType.Contains("BYTE"))
         {
            return "int";
         }
         else if(dataType.Contains("BIT"))
         {
            return "bit";
         }
         else if(dataType.Contains("BOOL"))
         {
            return "bit";
         }
         else if (dataType.Contains("DINT"))
         {
            return "bigint";
         }
         else if (dataType.Contains("INT"))
         {
            return "bigint";
         }
         else if (dataType.Contains("WORD"))
         {
            return "bigint";
         }
         else if (dataType.Contains("CHAR"))
         {
            return "char(1)";
         }
         else if (dataType.Contains("REAL"))
         {
            return "float";
         }
         else if (dataType.Contains("DATE"))
         {
            return "DateTime";
         }
         else if (dataType.Contains("TIME_OF_DAY"))
         {
            return "DateTime";
         }
         else if (dataType.Contains("STRING"))
         {
            return "nvarchar(64)";
         }
         
         if(warning)
         {
            warning = false;
            MessageBox.Show("Mindestens eine Spalte konnte nicht zugeordnet werden und wurde auf 'varbinary(MAX)' gestellt !", "IROPA DatenClieNT_TIA AuftragsGenerator", MessageBoxButtons.OK, MessageBoxIcon.Error);
         }

         return "varbinary(MAX)";
      }

      public int GetSpsDatatype(string dataType)
      {
         dataType = dataType.ToUpper();

         if (dataType.Contains("BYTE"))
         {
            return 8;
         }
         else if (dataType.Contains("BIT"))
         {
            return 6;
         }
         else if (dataType.Contains("BOOL"))
         {
            return 16;
         }
         else if (dataType.Contains("DINT"))
         {
            return 2;
         }
         else if (dataType.Contains("INT"))
         {
            return 1;
         }
         else if (dataType.Contains("WORD"))
         {
            return 1;
         }
         else if (dataType.Contains("REAL"))
         {
            return 7;
         }
         else if (dataType.Contains("CHAR"))
         {
            return 10;
         }
         else if (dataType.Contains("STRING"))
         {
            return 9;
         }
         else if (dataType.Contains("DATE_AND_TIME"))
         {
            //S7-DateTime 8-Byte
            return 14;
         }
         
         if (warning)
         {
            warning = false;
            MessageBox.Show(string.Format("Mindestens ein Datentyp ---{0}--- ist unbekannt und wurde auf SPS-Datentyp 7 (double) gestellt!", dataType), "IROPA DatenClieNT_TIA AuftragsGenerator", MessageBoxButtons.OK, MessageBoxIcon.Error);
         }

         return 7;
      }  
   }
}
