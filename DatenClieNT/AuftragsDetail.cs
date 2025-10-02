using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;

namespace DatenClieNT
{
   public class AuftragsDetail
   {
      private long id;
      private string tabellenSpalte;
      private string uaItem;
      private double wertFactor;      //Bei Bitoperationen auch als BitOffset mißbraucht
      private string statement = "";
      private string beschreibung = "";
      private object lastValue = System.DBNull.Value;
      private Boolean updateKriterium = false;
      private string sqlDatentyp = "";


      public string TabellenSpalte
      {
         get { return tabellenSpalte; }
      }

      public string SqlDatentyp
      {
         get { return sqlDatentyp; }
      }

      public SpsDatentyp SpsDatentyp
      {
         get { return SpsDatentyp.TIA; }
      }

      public int BlockAbgriff
      {
         get { return 0; }
      }

      public long Id
      {
         get { return id; }
      }

      public object LastValue
      {
         get { return lastValue; }
         set { lastValue = value; }
      }

      public string Statement
      {
         get { return statement; }
      }
      
      public double WertFactor
      {
         get{ return wertFactor; }
      }

      public string UaItem
      {
         get { return uaItem; }
      }

      public Boolean UpdateKriterium
      {
         get { return updateKriterium; }
      }
   
      public AuftragsDetail(Auftrag auftrag, long id, string tabellenSpalte, string beschreibung, string uaItem, double wertFactor, string sqlDatentyp)
      {
         this.updateKriterium = false;
         this.id = id;
         this.tabellenSpalte = tabellenSpalte;
         this.beschreibung = beschreibung;
         this.uaItem = uaItem;
         this.wertFactor = wertFactor;
         this.sqlDatentyp = sqlDatentyp;
      }

      public void CreateStatement11(string tabellenName, ref PseudoScript pseudoScript)
      {
         statement = string.Format("update {0} set {1} = @Wert", tabellenName, tabellenSpalte);
         
         foreach (string tabSpalte in pseudoScript.VariablenListe.Keys)
         {
            statement += string.Format(", {0} = {1}", tabSpalte, pseudoScript.VariablenListe[tabSpalte]);
         }

         //statement += string.Format(" where AuftragsDetail_ID = {0}\n", id);
         statement += string.Format(" where AuftragsDetail_ID = @AuftragsDetail_ID\n");
         
         statement += "if @@rowcount = 0 \nbegin\n";
         
         statement += string.Format("   insert into {0} (AuftragsDetail_ID, {1}", tabellenName, tabellenSpalte);

         foreach (string tabSpalte in pseudoScript.VariablenListe.Keys)
         {
            statement += string.Format(", {0}", tabSpalte);
         }

         //statement += string.Format(")\n   values ({0}, @Wert", id);
         statement += string.Format(")\n   values (@AuftragsDetail_ID, @Wert");
         
         

         foreach (string tabSpalte in pseudoScript.VariablenListe.Keys)
         {
            statement += string.Format(", {0}", pseudoScript.VariablenListe[tabSpalte]);
         }
         statement += ")\nend";
      }

      public void SetUpdateKrit()
      {
         updateKriterium = true;
      }
   }
}
