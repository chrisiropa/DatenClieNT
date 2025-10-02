using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;

namespace DatenClieNT
{
   public class Auftrag
   {
      /* 
      auftragsNummer und anlagenID 
      sind zusammen eindeutig
      AuftragsNummer kommt von der SPS in dem Daten-Strom (Nicht bei ClientAufträgen)
      Es sind also mehrere Aufträge mit der gleichen auftragsNummer zulässig, 
      wenn es verschiedene Anlagen sind !!!!!!!!
      */

      //Für Funktionsnummern gleich 1+2
      private List<string> queryParameterNames = new List<string>();
      private List<AuftragsDetail> queryParameterAuftragsDetails = new List<AuftragsDetail>();
      //Für Funktionsnummern gleich 1+2
      
      
      //Für Funktionsnummern ungleich 1+2
      private List<string> parameter;
      //Für Funktionsnummern ungleich 1+2
      
      private Int64 auftragsID;
      private Int64 datenClientID;
      private Int64 auftragsNummer;    
      private Int64 anlagenID;
      private FktNr funktionsNummer;
      private Int32 updateIntervall;
      private string tabellenName;
      private string bezeichnung;
      private PseudoScript pseudoScript;
      private string logTableForFN2;
      private Boolean lockID = false;
      private string sqlSuffix;
      private Boolean update = false;
      private object tag;
      private string database;
      
      public static object LockObject = new object();
      
      private string statement;
      
      private string logStatement;        //Kann es nur bei FN02RdRowEvtSPS geben
      private string createTableStatement;//Kann es nur bei FN02RdRowEvtSPS geben

      private List<Int64> updateKriteriums;
      private Dictionary<Int64, AuftragsDetail> auftragDetails;


      public List<string> QueryParameterNames
      {
         get { return queryParameterNames; }
      }
      public List<AuftragsDetail> QueryParameterAuftragsDetails
      {
         get { return queryParameterAuftragsDetails; }
      }
      public List<string> Parameter
      {
         get { return parameter; }
      }
      
      
      public Boolean LoggingTable
      {
         get { return (logTableForFN2 != ""); }
      }
      
      public Boolean LockID
      {
         get { return lockID; }
      }  

      public object Tag
      {
         get { return tag; }
      }

      public string LogStatement
      {
         get { return logStatement; }
      }

      

      
      public string Statement
      {
         get
         {
            return statement;
         }
      }

      public string CreateTableStatement
      {
         get
         {
            return createTableStatement;
         }
      }
      
      

      public PseudoScript PseudoScript
      {
         get { return pseudoScript; }
      }

      public Dictionary<Int64, AuftragsDetail> AuftragDetails
      {
         get { return auftragDetails; }
      }
         
      public string PrimaryKey
      {
         get 
         {
            return AuftragPrimaryKey(auftragsNummer, anlagenID);
         }
      }

      public FktNr FunktionsNummer
      {
         get { return funktionsNummer; }
      }
      
      public Boolean SmallTable
      {
         get
         {
            switch(funktionsNummer)
            {
               case FktNr.FN05WrColInsEvtSPS:
               case FktNr.FN11WrColUpdEvtDC:
               case FktNr.FN13WrColInsEvtDC:
               //Es handelt sich um eine schmale Datentabelle
               //Hier MUSS es eine Spalte AuftragsDetail_ID geben,
               //sonst funktioniert es nicht.
               return true;
            }
            return false;
         }
      }

      public Int32 UpdateIntervall
      {
         get { return updateIntervall; }
      }

      public string TabellenName
      {
         get { return tabellenName; }
      }

      public string LogTableForFN2
      {
         get { return logTableForFN2; }
      }
      
      
      public string Bezeichnung
      {
         get 
         {
            return string.Format("AuftragsNummer={0} Anlage={1} Funktion={2}", auftragsNummer, anlagenID, funktionsNummer);
         }
      }
      
      public Int64 AuftragsNummer
      {
         get { return auftragsNummer; }
      }
      
      public Int64 AnlagenID
      {
         get { return anlagenID; }
      }
      
      
      public static string AuftragPrimaryKey(Int64 auftragsNummer, Int64 anlagenID)
      {
         return string.Format("{0}:{1}", auftragsNummer, anlagenID);
      }

      private Dictionary<string, string> ExtractSqlDatentyp(string tabellenName)
      {
         if(tabellenName.Contains("(") && tabellenName.Contains(")"))
         {
            //Die Tabelle ist keine Tabelle und kein View sondern eine Function
				//CG: 29.04.2022 Bei Keula wurde die Funktion mit einem Parameter aufgerufen. Dieser ist vorher übrig geblieben und störte.
				//Nun wird alles ab der ersten Klammer abgeschnitten, um den Namen der Funktion zu ermitteln.
				tabellenName = tabellenName.Substring(0, tabellenName.IndexOf('('));
            //tabellenName = tabellenName.Replace("(", "");
            //tabellenName = tabellenName.Replace(")", "");
         }


         Dictionary<string, string> datentypen = new Dictionary<string, string>();

         SqlRealtimeSimpleQuery query = null;
         string statement = string.Format("SELECT sysobjects.name, syscolumns.name as columnname, systypes.name as datatype, syscolumns.length, systypes.usertype FROM sysobjects JOIN syscolumns ON sysobjects.[id] = syscolumns.[id] JOIN systypes ON syscolumns.xtype = systypes.xtype AND systypes.usertype < 100 and systypes.name not in ('sysname') WHERE sysobjects.name = '{0}'", tabellenName);

         try
         {
            query = new SqlRealtimeSimpleQuery(database, statement);

            if (query.QueryResult != null)
            {
               foreach (Dictionary<string, object> prm in query.QueryResult)
               {
                  datentypen[DbTools.GetString(prm, "columnname").ToUpper()] = DbTools.GetString(prm, "datatype").ToUpper();
               }
            }
         }
         catch (Exception e)
         {
            LogManager.GetSingleton().ZLog("CD337", ELF.ERROR, "ExtractSqlDatentyp -> {0} -> Query = {1}", e.Message, statement);
         }

         return datentypen;
      }

      public Auftrag(string database, int maxLenBezeichnung, object parent, Int64 auftragsID, Int64 datenClientID, Int64 auftragsNummer, Int64 anlagenID, string bezeichnung, FktNr funktionsNummer, string tabellenName, string updateKriterium, Int32 updateIntervall, string pseudoScript, string logTableForFN2, object tag, string sqlSuffix, Boolean lockID)
      {
         this.lockID = lockID;
         this.update = false;
         this.auftragsID = auftragsID;
         this.datenClientID = datenClientID;
         this.anlagenID = anlagenID;
         this.auftragsNummer = auftragsNummer;
         this.funktionsNummer = funktionsNummer;
         this.tabellenName = tabellenName.Trim();
         this.updateIntervall = updateIntervall;
         this.bezeichnung = bezeichnung;
         this.pseudoScript = new PseudoScript(pseudoScript);
         this.logTableForFN2 = logTableForFN2;
         this.tag = tag;
         this.sqlSuffix = sqlSuffix;
         this.database = database;
                  
         this.updateKriteriums = new List<long>();

         Dictionary<string, string> datenTypen = ExtractSqlDatentyp(tabellenName);

         if (datenTypen == null)
         {
            throw new Exception(string.Format("Auftrag {0} ungültig. SqlDatentypen konnten nicht ermittelt werden !", auftragsNummer));
         }


         string [] texte = updateKriterium.Split(',');
         
         foreach(string text in texte)
         {
            if(text.Length > 0)
            {
               updateKriteriums.Add(Convert.ToInt64(text));
            }
         }

         string auftragText = string.Format("Name={0} Funktion={1} Tabelle={2}", bezeichnung.PadRight(maxLenBezeichnung, ' '), EnumHelper.FktNrText(funktionsNummer), tabellenName.PadRight(28, ' '));
         
         auftragDetails = new Dictionary<Int64, AuftragsDetail>();

         SqlRealtimeSimpleQuery query = null;
         string statement = string.Format("select Convert(bigint, TS) as TS, * from DC_AuftragsDetailsTIA where Auftrags_ID = {0} order by ID", auftragsID);
         
         try
         {
            query = new SqlRealtimeSimpleQuery(database, statement);

            if (query.QueryResult != null)
            {
               foreach (Dictionary<string, object> prm in query.QueryResult)
               {
                  long auftragDetailsID = DbTools.GetInt64(prm, "ID");
                  long ts = Convert.ToInt64(prm["TS"]);
                  string tabellenSpalte = DbTools.GetString(prm, "TabellenSpalte").Trim();
                  string beschreibung = DbTools.GetString(prm, "Beschreibung");
                  string uaItem = DbTools.GetString(prm, "UA_Item");
                  double wertFactor = DbTools.GetDouble(prm, "WertFactor");

                  if (!datenTypen.ContainsKey(tabellenSpalte.ToUpper()))
                  {
                     LogManager.GetSingleton().ZLog("CD33A", ELF.ERROR, "Tabellenspalte {0} nicht in der Tabelle {1} vorhanden. Konfigurationsfehler !", tabellenSpalte, tabellenName);
                     throw new Exception(string.Format("Auftrag {0} ungültig !", auftragsNummer));
                  }

                  if(auftragDetailsID == 1123025)
                  {
                  }

                  auftragDetails[auftragDetailsID] = new AuftragsDetail(this, auftragDetailsID, tabellenSpalte, beschreibung, uaItem, wertFactor, datenTypen[tabellenSpalte.ToUpper()]);


                     
               
                  if(updateKriteriums.Contains(auftragDetailsID))
                  {
                     auftragDetails[auftragDetailsID].SetUpdateKrit();
                     update = true;
                  }
               }
            }
         }
         catch (Exception e)
         {
            LogManager.GetSingleton().ZLog("C004F", ELF.ERROR, "AuftragSPS -> {0} -> Query = {1}", e.Message, statement);
            
            throw new Exception(string.Format("Auftrag {0} ungültig !", auftragsNummer));
         }

         if(!CreateStatement())
         {
            throw new Exception(string.Format("Auftrag {0} ungültig (CreateStatement) !", auftragsNummer));
         }
      }
      
      private Boolean CreateStatement()
      {
         parameter = new List<string>();
         
         int paramCounter = 0;
         
         if(auftragDetails.Count < 1)
         {
            LogManager.GetSingleton().ZLog("C0050", ELF.INFO, "Keine Items für Auftrag {0} definiert. DatenClient = {1}", Bezeichnung, datenClientID);
            //Kein Abbruch nötig, da evtl. konfigurierte Pseudovariablen ausreichen würden
         }
         
         if(!pseudoScript.Parse())
         {
            LogManager.GetSingleton().ZLog("C0051", ELF.ERROR, "Auftrag.CreateStatement: Parsen des PseudoScripts fehlgeschlagen: Auftrag = {0}. Das SQL-Statement für Auftrag konnte nicht generiert werden.", bezeichnung);
            return false;
         }

         if ((auftragDetails.Count < 1) && (pseudoScript.VariablenListe.Count == 0))
         {
            LogManager.GetSingleton().ZLog("C0052", ELF.ERROR, "Keine Items für Auftrag {0} definiert und auch keine Pseudovariablen !. DatenClient = {1}", Bezeichnung, datenClientID);
            return false;
         }
      
         if(funktionsNummer == FktNr.FN01WrRowInsEvtSPS)
         {
            parameter.Add("Wert");
            
            statement = "";
            string komma = "";
            
            if(update)
            {            
               /*
               Auftragsdetail des Kriteriums besorgen 
               */
               
               
               statement = string.Format("update {0} set \n", tabellenName);
                              
               foreach (AuftragsDetail auftragsDetail in auftragDetails.Values)
               {
                  paramCounter++;
                  
                  if (!this.updateKriteriums.Contains(auftragsDetail.Id))
                  {               
                     statement += komma;
                     statement += auftragsDetail.TabellenSpalte;
                     statement += string.Format(" = @Wert{0} ", paramCounter);                  
                     komma = ", ";
                  }
                  

                  if (this.updateKriteriums.Contains(auftragsDetail.Id))
                  {
                     queryParameterNames.Add(string.Format("@Wert{0}", paramCounter));
                     queryParameterAuftragsDetails.Add(auftragsDetail);
                  }
                  
                  
               }
               
               foreach(string tabellenSpalte in pseudoScript.VariablenListe.Keys)
               {
                  statement += komma;
                  statement += tabellenSpalte;
                  statement += string.Format(" = {0}", pseudoScript.VariablenListe[tabellenSpalte]);
                  komma = ", ";
               }

               statement += string.Format(" \nwhere ");
               
               //Wenn AnlagenID in PseudoScript, dann im where mit berücksichtigen
               
               int index = 0;
               string and = "";
               foreach (string parameterName in queryParameterNames)
               {
                  statement += string.Format("\n{2} {0} = {1}", queryParameterAuftragsDetails[index].TabellenSpalte, parameterName, and);

                  and = " and ";

                  index++;
               }
               
               
               //Folgende Passage ist dazu da, die AnlagenID zu berücksichtigen, falls sie im 
               //PseudoScript angegeben ist. Ansonsten wären Einträge in z.B. Prop-Tabellen
               //wo die Prop-Nr nur zusammen mit der AnlagenID eindeutig ist nur dann möglich
               //wenn die SPS die automatisch vergebene ID der Prop-Tabelle kennen würde.
               //Tut sie aber nicht.
               foreach (string spaltenName in pseudoScript.VariablenListe.Keys)
               {
                  //Der Spaltenname ist der Key, Die Variable ist der Value
                  if (pseudoScript.VariablenListe[spaltenName] == "@ANLAGEN_ID")
                  {
                     statement += string.Format("\n{2} {0} = {1}", spaltenName, "@ANLAGEN_ID", and);
                     break;
                  }
               }
               

               statement += string.Format("\n");          
               statement += "if @@rowcount = 0 begin";
               statement += string.Format("\n");
            }
            
            //hier insert 
            paramCounter = 0;
            komma = "";
            statement += string.Format("insert into {0} \n(", tabellenName);

            foreach (AuftragsDetail auftragsDetail in auftragDetails.Values)
            {
               statement += komma;
               statement += auftragsDetail.TabellenSpalte;
               komma = ", ";
            }

            foreach (string tabellenSpalte in pseudoScript.VariablenListe.Keys)
            {
               statement += komma;
               statement += tabellenSpalte;
               komma = ", ";
            }
            
            statement += ") \nvalues (";

            komma = "";

            foreach (AuftragsDetail auftragsDetail in auftragDetails.Values)
            {
               statement += komma;
               statement += string.Format("@Wert{0} ", ++paramCounter);
               komma = ", ";
            }

            foreach (string tabellenSpalte in pseudoScript.VariablenListe.Keys)
            {
               statement += komma;
               statement += string.Format(" {0} ", pseudoScript.VariablenListe[tabellenSpalte]);
               komma = ", ";
            }

            statement += ")";
            
            if(update)
            {
               statement += string.Format("\nend;");
            }      
            

            
         }
         else if (funktionsNummer == FktNr.FN05WrColInsEvtSPS)
         {
            parameter.Add("AuftragsDetail_ID");
            parameter.Add("Wert");
            statement = "";
            
            paramCounter = 0;
            
            foreach (AuftragsDetail auftragsDetail in auftragDetails.Values)
            {
               statement += string.Format("insert into {0} (AuftragsDetail_ID, {1}", tabellenName, auftragsDetail.TabellenSpalte);

               foreach (string tabellenSpalte in pseudoScript.VariablenListe.Keys)
               {
                  statement += string.Format(", {0}", tabellenSpalte);
               }
               
               statement += ")\n";

               
               statement += string.Format("select @{0}{2}, @{1}{2}", parameter[0], parameter[1], ++paramCounter);

               foreach (string tabellenSpalte in pseudoScript.VariablenListe.Keys)
               {
                  statement += string.Format(", {0}", pseudoScript.VariablenListe[tabellenSpalte]);
               }

               statement += ";\n";
            }

            //paramCounter     
         }
         else if (funktionsNummer == FktNr.FN11WrColUpdEvtDC)
         {
            //Bei Funktionsnummer 11 beinhaltet das AuftragsDetail das Statement
            statement = "";
            
            foreach (AuftragsDetail auftragsDetail in auftragDetails.Values)
            {
               auftragsDetail.CreateStatement11(tabellenName, ref pseudoScript);
            }
         }
         else if ((funktionsNummer == FktNr.FN02RdRowEvtSPS) || (funktionsNummer == FktNr.FN20RdRowEvtSPS))
         {
            string komma = "";
            Boolean functionMode = false;
            string potentialFunctionParameter = "";
            
            if(tabellenName.Contains("(") && tabellenName.Contains(")"))
            {
               //Die Tabelle ist keine Tabelle und kein View sondern eine Function
               functionMode = true;
               tabellenName = tabellenName.Replace("(", "");
               tabellenName = tabellenName.Replace(")", "");
            }
            
            if(funktionsNummer == FktNr.FN20RdRowEvtSPS)
            {
               statement = string.Format("select ");
            }
            else
            {
               statement = string.Format("select top 1 ");
            }
            
            logStatement = string.Format("insert into {0} (", logTableForFN2);
            
            logStatement += string.Format("LOG_DateTime, ");
            
            foreach (AuftragsDetail auftragsDetail in auftragDetails.Values)
            {
               if(this.updateKriteriums.Contains(auftragsDetail.Id))
               {
                  potentialFunctionParameter += string.Format("@Wert{0}", paramCounter);
                  queryParameterNames.Add(string.Format("@Wert{0}", paramCounter++));
                  queryParameterAuftragsDetails.Add(auftragsDetail);
               }

               statement += komma;
               logStatement += komma;
               
               statement += string.Format(" {0}", auftragsDetail.TabellenSpalte);
               logStatement += string.Format(" {0}", auftragsDetail.TabellenSpalte);

               komma = ", ";
            }
            
            if(!functionMode)
            {
               statement += string.Format(" from {0} \nwhere ", tabellenName);               
            }
            else
            {
               statement += string.Format(" from {0}({1}) \n ", tabellenName, potentialFunctionParameter);               
            }

            logStatement += string.Format(") ");
            
            int index = 0;            
            string and = "";
            foreach (string parameterName in queryParameterNames)
            {
               if (!functionMode)
               {
                  statement += string.Format("\n{2} {0} = {1}", queryParameterAuftragsDetails[index].TabellenSpalte, parameterName, and);            
                  and = " and ";
               }
               else
               {
                  //Kein where-Statement im FunctionModus !!!
               }
               
               index++;
            }


            //Folgende Passage ist dazu da, die AnlagenID zu berücksichtigen, falls sie im 
            //PseudoScript angegeben ist. Ansonsten wären Einträge in z.B. Prop-Tabellen
            //wo die Prop-Nr nur zusammen mit der AnlagenID eindeutig ist nur dann möglich
            //wenn die SPS die automatisch vergebene ID der Prop-Tabelle kennen würde.
            //Tut sie aber nicht.
            foreach (string spaltenName in pseudoScript.VariablenListe.Keys)
            {
               //Der Spaltenname ist der Key, Die Variable ist der Value
               if (pseudoScript.VariablenListe[spaltenName] == "@ANLAGEN_ID")
               {
                  statement += string.Format("\n{2} {0} = {1}", spaltenName, "@ANLAGEN_ID", and);
                  break;
               }
            }
            
            if(functionMode)
            {
            }
            
            
            statement += string.Format(" {0}", sqlSuffix);

            logStatement += string.Format("select @LOG_DateTime, {0}", statement.Replace("select ", ""));
            
            if(!LoggingTable)
            {
               createTableStatement = "";
               logStatement = "";
            }
            else
            {
               createTableStatement = string.Format("IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'{0}') AND type in (N'U'))", logTableForFN2);
               createTableStatement += System.Environment.NewLine;
               createTableStatement += "BEGIN";
               createTableStatement += System.Environment.NewLine;               
               createTableStatement += string.Format("select * into {0} from {1} where 0 = 1;", logTableForFN2, tabellenName);
               createTableStatement += System.Environment.NewLine;
               createTableStatement += string.Format("alter table {0} add LOG_DateTime DateTime not NULL;", logTableForFN2);
               createTableStatement += System.Environment.NewLine;
               createTableStatement += string.Format("alter table {0} add LOG_ID [bigint] IDENTITY(1,1) NOT NULL;", logTableForFN2);
               createTableStatement += System.Environment.NewLine;
               createTableStatement += string.Format("alter table {0} add CONSTRAINT [PK_{0}] PRIMARY KEY CLUSTERED ", logTableForFN2);
               createTableStatement += System.Environment.NewLine;
               createTableStatement += "(";
               createTableStatement += System.Environment.NewLine;
               createTableStatement += "LOG_ID ASC";
               createTableStatement += System.Environment.NewLine;
               createTableStatement += ")";
               createTableStatement += System.Environment.NewLine;
               createTableStatement += "WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]";
               createTableStatement += System.Environment.NewLine;
               createTableStatement += string.Format("CREATE NONCLUSTERED INDEX IX_LOG_LEG_SPS_Uebergabe ON {0}", logTableForFN2);
               createTableStatement += System.Environment.NewLine;
               createTableStatement += "(";
               createTableStatement += System.Environment.NewLine;
	            createTableStatement += "LOG_DateTime DESC";
               createTableStatement += System.Environment.NewLine;
               createTableStatement += ")";
               createTableStatement += System.Environment.NewLine;
               createTableStatement += "WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]";
               createTableStatement += System.Environment.NewLine;
               createTableStatement += "END;";
               createTableStatement += System.Environment.NewLine;
               createTableStatement += "GO";
               createTableStatement += System.Environment.NewLine;



               ExecuteScript executeScript = new ExecuteScript();
               
               try
               {
                  if(executeScript.Execute(database, createTableStatement))
                  {
                     LogManager.GetSingleton().ZLog("C0053", ELF.INFO, "Script für Logtabelle erfolgreich ausgeführt !");
                  }
                  else
                  {
                     LogManager.GetSingleton().ZLog("C0054", ELF.ERROR, "Script für Logtabelle fehlgeschlagen ! -> {0} statement = {1}", executeScript.ErrorText, createTableStatement);
                  }
               }
               catch(Exception e)
               {
                  LogManager.GetSingleton().ZLog("C0055", ELF.ERROR, "Auftrag. CreateStatement() -> {0}", e.Message);
               }
            }
            
            
            
            if(!update)
            {   
               //statement = "";         
            }          
            
            
         }
         else if (funktionsNummer == FktNr.FN12WrRowInsEvtDC)
         {
            parameter.Add("Wert");

            statement = "";
            string komma = "";

            if (update)
            {
               /*
               Auftragsdetail des Kriteriums besorgen 
               */


               statement = string.Format("update {0} set \n", tabellenName);

               foreach (AuftragsDetail auftragsDetail in auftragDetails.Values)
               {
                  paramCounter++;

                  if (!this.updateKriteriums.Contains(auftragsDetail.Id))
                  {
                     statement += komma;
                     statement += auftragsDetail.TabellenSpalte;
                     statement += string.Format(" = @Wert{0} ", paramCounter);
                     komma = ", ";
                  }


                  if (this.updateKriteriums.Contains(auftragsDetail.Id))
                  {
                     queryParameterNames.Add(string.Format("@Wert{0}", paramCounter));
                     queryParameterAuftragsDetails.Add(auftragsDetail);
                  }


               }

               foreach (string tabellenSpalte in pseudoScript.VariablenListe.Keys)
               {
                  statement += komma;
                  statement += tabellenSpalte;
                  statement += string.Format(" = {0}", pseudoScript.VariablenListe[tabellenSpalte]);
                  komma = ", ";
               }

               statement += string.Format(" \nwhere ");

               //Wenn AnlagenID in PseudoScript, dann im where mit berücksichtigen

               int index = 0;
               string and = "";
               foreach (string parameterName in queryParameterNames)
               {
                  statement += string.Format("\n{2} {0} = {1}", queryParameterAuftragsDetails[index].TabellenSpalte, parameterName, and);

                  and = " and ";

                  index++;
               }


               statement += string.Format("\n");
               statement += "if @@rowcount = 0 begin";
               statement += string.Format("\n");
            }

            //hier insert 
            paramCounter = 0;
            komma = "";
            statement += string.Format("insert into {0} \n(", tabellenName);

            foreach (AuftragsDetail auftragsDetail in auftragDetails.Values)
            {
               statement += komma;
               statement += auftragsDetail.TabellenSpalte;
               komma = ", ";
            }

            foreach (string tabellenSpalte in pseudoScript.VariablenListe.Keys)
            {
               statement += komma;
               statement += tabellenSpalte;
               komma = ", ";
            }

            statement += ") \nvalues (";

            komma = "";

            foreach (AuftragsDetail auftragsDetail in auftragDetails.Values)
            {
               statement += komma;
               statement += string.Format("@Wert{0} ", ++paramCounter);
               komma = ", ";
            }

            foreach (string tabellenSpalte in pseudoScript.VariablenListe.Keys)
            {
               statement += komma;
               statement += string.Format(" {0} ", pseudoScript.VariablenListe[tabellenSpalte]);
               komma = ", ";
            }

            statement += ")";

            if (update)
            {
               statement += string.Format("\nend;");
            } 
         }
         else if (funktionsNummer == FktNr.FN13WrColInsEvtDC)
         {
            parameter.Add("AuftragsDetail_ID");
            parameter.Add("Wert");
            statement = "";

            paramCounter = 0;

            foreach (AuftragsDetail auftragsDetail in auftragDetails.Values)
            {
               statement += string.Format("insert into {0} (AuftragsDetail_ID, {1}", tabellenName, auftragsDetail.TabellenSpalte);

               foreach (string tabellenSpalte in pseudoScript.VariablenListe.Keys)
               {
                  statement += string.Format(", {0}", tabellenSpalte);
               }

               statement += ")\n";


               statement += string.Format("select @{0}{2}, @{1}{2}", parameter[0], parameter[1], ++paramCounter);

               foreach (string tabellenSpalte in pseudoScript.VariablenListe.Keys)
               {
                  statement += string.Format(", {0}", pseudoScript.VariablenListe[tabellenSpalte]);
               }

               statement += ";\n";
            }    
         }
         else
         {
            LogManager.GetSingleton().ZLog("C0056", ELF.ERROR, "Auftrag.CreateStatement: Funktionsnummer wird noch nicht unterstützt -> {0} Auftrag={1}", funktionsNummer, bezeichnung);
            return false;
         }
         
         return true;
      }
   }
}
