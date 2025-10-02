using System;
using System.Collections.Generic;
using System.Text;

namespace DatenClieNT_AuftragsGenerator
{
   public class StatementAuftrag
   {
   
      private string id;
      private string auftragsNummer;
      private string datenclientID;
      private string bezeichnung;
      private string view;
      private string pseudoScript;
      private string funktionsnummer;

      private string blockTag;
      private string updateKriterium;
      private string detailPrefix;
      private string updateIntervall;
      private string logTableForFN2;
      private string tagValues;

      private const string varID = "#ID";
      private const string varAuftragsNummer = "#AuftragsNummer";
      private const string varDatenclientID = "#DatenclientID";
      private const string varBezeichnung = "#Bezeichnung";
      private const string varView = "#View";
      private const string varPseudoScript = "#Pseudoscript";
      private const string varFunktionsnummer = "#Funktionsnummer";
      private const string varUpdateKriterium = "#UpdateKriterium";
      private const string varBlockTag = "#BlockTag";
      private const string varUpdateIntervall = "#UpdateIntervall";
      private const string varLogTableForFN2 = "#LogTableForFN2";
      private const string varTagvalues = "#Tagvalues";


      private string templateAuftrag = string.Format("insert into DC_AufträgeTIA (ID, Datenclient_ID, AuftragsNummer, Aktiv, Bezeichnung, FunktionsNummer, TabellenName, UpdateKriterium, BlockTag, UpdateIntervall, Pseudoscript, LogTableForFN2, Tag) values ({0}, {1}, {2}, 1, '{3}', {4}, '{5}', '{6}', '{7}', {8}, '{9}', '{10}', '{11}')", varID, varDatenclientID, varAuftragsNummer, varBezeichnung, varFunktionsnummer, varView, varUpdateKriterium, varBlockTag, varUpdateIntervall, varPseudoScript, varLogTableForFN2, varTagvalues);
   
      private string statement = "";
      
      public string Statement
      {
         get { return statement; }
      }

      private void Go()
      {
         statement += Environment.NewLine;
         statement += "GO";
         statement += Environment.NewLine;
      }

      public StatementAuftrag(string id, string auftragsNummer, string datenclientID, string bezeichnung, string view, string pseudoScript, string funktionsnummer, string blockTag, string updateKriterium, string updateIntervall, string logTableForFN2, string tagValues, string detailPrefix)
      {
         statement = "";
         
         this.detailPrefix = detailPrefix;
         this.id = id;
         this.auftragsNummer = auftragsNummer;
         this.datenclientID = datenclientID;
         this.bezeichnung = bezeichnung;
         this.view = view;
         this.pseudoScript = pseudoScript;
         this.funktionsnummer = funktionsnummer;
         this.blockTag = blockTag;
         this.updateKriterium = updateKriterium;
         this.updateIntervall = updateIntervall;
         this.logTableForFN2 = logTableForFN2;
         this.tagValues = tagValues;
            
         Generate();
      }

      private void Generate()
      {
         statement = "";

         statement = templateAuftrag.Replace(varID, id);
         statement = statement.Replace(varAuftragsNummer, auftragsNummer);
         statement = statement.Replace(varDatenclientID, datenclientID);
         statement = statement.Replace(varBezeichnung, bezeichnung);
         statement = statement.Replace(varView, view);
         statement = statement.Replace(varPseudoScript, pseudoScript);
         statement = statement.Replace(varFunktionsnummer, funktionsnummer);
         statement = statement.Replace(varUpdateKriterium, updateKriterium);
         statement = statement.Replace(varBlockTag, blockTag);
         
         try
         {
            Convert.ToInt32(updateIntervall);
            statement = statement.Replace(varUpdateIntervall, updateIntervall);
         }
         catch
         {
            statement = statement.Replace(varUpdateIntervall, "NULL");
         }
         
         
         statement = statement.Replace(varLogTableForFN2, logTableForFN2);
         statement = statement.Replace(varTagvalues, tagValues);
         
         Go();
      }
   }
}
