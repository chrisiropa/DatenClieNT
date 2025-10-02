using System;
using System.Collections.Generic;
using System.Text;

namespace DatenClieNT_AuftragsGenerator
{
   public class StatementTabelle
   {
      //Für Tabellengenerierung
      private const string varNameTabellenname = "#Tabellenname";
      private const string varNameSpaltenname = "#Spaltenname";
      private const string varNameKommentar = "#Kommentar";
      private const string varNameSqlDataType = "#SqlDataType";
      
      //Für AuftragsDetailgenerierung
      private const string varID = "#ID";
      private const string varAuftragsID = "#AuftragsID";
      private const string varSpaltenname = "#Spaltenname";
      private const string varSpsDatentyp = "#SpsDatentyp";
      private const string varFaktor = "#Faktor";
      private const string varBeschreibung = "#Beschreibung";
      private const string varUA_Item = "#UA_Item";

      private string templateDrop = string.Format("IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[{0}]') AND type in (N'U')) DROP TABLE [dbo].[{0}]", varNameTabellenname);
      private string templateCreateAutoPK = string.Format("SET ANSI_NULLS ON\nGO\nSET QUOTED_IDENTIFIER ON\nGO\nCREATE TABLE [dbo].[{0}] ([ID] [bigint] IDENTITY(1,1) NOT NULL, CONSTRAINT [PK_{0}] PRIMARY KEY CLUSTERED ([ID] ASC) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]) ON [PRIMARY]", varNameTabellenname);
      private string templateAddColumn = string.Format("alter table [dbo].[{0}] add {1} {2} NULL", varNameTabellenname, varNameSpaltenname, varNameSqlDataType);            
      private string templateComment = string.Format("EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'{0}' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'{1}', @level2type=N'COLUMN',@level2name=N'{2}'", varNameKommentar, varNameTabellenname, varNameSpaltenname);
      private string templateNotUniqueIndex = string.Format("IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[{0}]') AND name = N'IX_{0}_{1}')\nDROP INDEX [IX_{0}_{1}] ON [dbo].[{0}] WITH ( ONLINE = OFF )\nGO\nCREATE NONCLUSTERED INDEX [IX_{0}_{1}] ON [dbo].[{0}]\n(\n   [{1}] ASC\n) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]\nGO\n", varNameTabellenname, varNameSpaltenname);

      private string templateAuftragsDetail = string.Format("insert into DC_AuftragsDetailsTIA (ID, Auftrags_ID, TabellenSpalte,	WertFactor, Beschreibung, UA_Item) select {0}, {1}, '{2}', {3}, '{4}', '{5}'", varID, varAuftragsID, varSpaltenname, varFaktor, varBeschreibung, varUA_Item);            
            
      //Eingabe
      private char trennzeichen;
      private string tabellenName;
      private string substALT;
      private string substNEU;
      private string detailPrefix;
      private Boolean autoID;
      private string datumZeitSpalte;
      private string rohDaten;
      private string auftragsID;
      private int initialAuftragsDetailID;
      private int auftragsDetailCounter = 0;

      private DatatypeRecognizer datatypeRecognizer = new DatatypeRecognizer();
      
      //Ausgabe            
      private string statement = "";
      
      public string Statement
      {
         get { return statement; }
      }

      private string statementAuftragDetails = "";

      public string StatementAuftragDetails
      {
         get { return statementAuftragDetails; }
      }
      
      public StatementTabelle(string rohDaten, string tabellenName, char trennzeichen, string substALT, string substNEU, Boolean autoID, string datumZeitSpalte, string auftragsID, string detailPrefix)
      {
         this.rohDaten = rohDaten;
         this.tabellenName = tabellenName;
         this.trennzeichen = trennzeichen;
         this.substALT = substALT;
         this.substNEU = substNEU;
         this.detailPrefix = detailPrefix;
         this.autoID = autoID;
         this.datumZeitSpalte = datumZeitSpalte;
         this.auftragsID = auftragsID;

         initialAuftragsDetailID = Convert.ToInt32(string.Format("{0}000", auftragsID));
         
         statement = "";
         
         Generate();
      }
      
      private void Go()
      {
         statement += Environment.NewLine;
         statement += "GO";
         statement += Environment.NewLine;
      }

      private void Generate()
      {
         statement = "";
         
         //Tabelle droppen... 	      
 	      statement += templateDrop.Replace(varNameTabellenname, tabellenName);
         Go();

         //Tabelle erzeugen... 	
         statement += templateCreateAutoPK.Replace(varNameTabellenname, tabellenName);
         Go();

         if(this.datumZeitSpalte.Length > 0)
         {         
            AddColumn(this.datumZeitSpalte, "DateTime", "Standard-Datumzeitspalte", true);            
         }
         
 	      //Spalten und deren Kommentare hinzufügen...
         DatatypeRecognizer datatypeRecognizer = new DatatypeRecognizer();
         
 	      foreach(string rohZeile in rohDaten.Split('\n'))
 	      {
 	         string line = rohZeile.Replace("\r", ""); 	         
 	         
 	         if(line.Length < 1)
 	         {
 	            continue;
 	         }
 	         
 	         string [] parts = line.Split(trennzeichen);
 	         
 	         string adresse = "";
 	         string spaltenname = "";
            string datentyp = "";
            string kommentar = "";
            
            int index = 0;
            foreach(string part in parts)
            {
               switch(index)
               {
                  case 0: spaltenname = part; break;
                  case 1: adresse = part; break;
                  case 2: datentyp = part; break;
                  case 3: kommentar = part; break;
               }
               
               if(index > 3)
               {
                  //Falls in den Kommentaren das Trennzeichen vorkommt...
                  kommentar += part;
               }
            
               index++;
            }
            
            if(spaltenname.Length < 1)
            {
               continue;
            }
            
            if(substALT.Length > 0)
            {            
               spaltenname = spaltenname.Replace(substALT, substNEU);
            }
            
            string bereinigterSpaltenname = AddColumn(spaltenname, datatypeRecognizer.GetSqlDatatype(datentyp), kommentar, false);

            string komplettesItem = string.Format("{0}{1}", detailPrefix, adresse);

            AddAuftragsDetail(komplettesItem, bereinigterSpaltenname, datentyp, kommentar);
 	      } 	      
      }
      
      

      private void AddAuftragsDetail(string adresse, string spaltenname, string datentyp, string kommentar)
      {
         int neueAuftragsDetailID = initialAuftragsDetailID + (auftragsDetailCounter++);
      
         string tempStatementAuftragDetails = templateAuftragsDetail.Replace(varID, neueAuftragsDetailID.ToString());
         tempStatementAuftragDetails = tempStatementAuftragDetails.Replace(varAuftragsID, auftragsID);
         tempStatementAuftragDetails = tempStatementAuftragDetails.Replace(varSpaltenname, spaltenname);
         tempStatementAuftragDetails = tempStatementAuftragDetails.Replace(varBeschreibung, kommentar);
         
         int spsDatentyp = datatypeRecognizer.GetSpsDatatype(datentyp);

         tempStatementAuftragDetails = tempStatementAuftragDetails.Replace(varSpsDatentyp, spsDatentyp.ToString());
         
         
      
         tempStatementAuftragDetails = tempStatementAuftragDetails.Replace(varFaktor, "1");
         tempStatementAuftragDetails = tempStatementAuftragDetails.Replace(varUA_Item, adresse);            
         

         tempStatementAuftragDetails += Environment.NewLine;
         
         statementAuftragDetails += tempStatementAuftragDetails;
      }
      
      
      private string AddColumn(string columnName, string sqlDatentyp, string comment, Boolean index)
      {
         columnName = columnName.Replace('.', '_');
         columnName = columnName.Replace(' ', '_');
         columnName = columnName.Replace('[', '_');
         columnName = columnName.Replace(']', '_');
         columnName = columnName.Replace('(', '_');
         columnName = columnName.Replace(')', '_');
      
         statement += templateAddColumn.Replace(varNameSpaltenname, columnName).Replace(varNameTabellenname, tabellenName).Replace(varNameSqlDataType, sqlDatentyp);
         Go();
         statement += templateComment.Replace(varNameSpaltenname, columnName).Replace(varNameTabellenname, tabellenName).Replace(varNameKommentar, comment);
         Go();  
         
         if(index)
         {
            //index anlegen
            statement += templateNotUniqueIndex.Replace(varNameSpaltenname, columnName).Replace(varNameTabellenname, tabellenName);
            Go();
         }  
         
         return columnName;
      }
   }
}
