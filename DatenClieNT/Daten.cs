using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;

namespace DatenClieNT
{
   abstract class Daten
   {
      protected int firstDTelegramsIgnoreCounter = 0;
      
      protected int headerLength = 40;
      private UInt16 mTelHeaderLen = 40;
   
      protected Dictionary<string, Auftrag> spsAuftraege;
      protected Dictionary<string, Auftrag> clientAuftraege;
      
      protected long datenClientID;
      protected string datenClientName;
      protected string datenbank;
      protected long anlagenID;
      protected Dictionary<long, long> nebenAnlagen;
      private DataAcknoledge dataAcknoledge = new DataAcknoledge(0, SpsReturnCodes.Ok);
      protected bool readTelegramE = false;
		protected DatenClient datenClient = null;
      
      protected Boolean diffIDsDandE = false;
      protected UInt16 lastIndexD = 0;
      
      public Dictionary<string, Auftrag> SpsAuftraege
      {
         get { return spsAuftraege; }
      }
      
      public DataAcknoledge DataAcknoledge
      {
         get
         {
            return dataAcknoledge;
         }
      } 

      public Boolean DiffIDsDandE
      {
         set { diffIDsDandE = value; }
         get { return diffIDsDandE; }
      }
      
      protected UInt16 MTelHeaderLen
      {
         get { return mTelHeaderLen; }
      }

      
      
      public bool ReadTelegramE
      {
         get { return readTelegramE; }
      }
      
      public Dictionary<string, Auftrag> ClientAuftraege
      {
         get { return clientAuftraege; }
      }
      
      public virtual void HandleTelegramD(TelegramD telegramD)
      {
         if(firstDTelegramsIgnoreCounter > 0)
         {
            LogManager.GetSingleton().ZLog("C0069", ELF.INFO, "D-Telegram per Definition ignoriert...");
            firstDTelegramsIgnoreCounter--;
         }

         
         
         readTelegramE = false;
         
         //Datenempfang wird signalisiert !         
         DataAcknoledge.SetSpsReturnCode(SpsReturnCodes.Ok);
         
         telegramD.IndexTelegramD = Convert.ToUInt16(telegramD.SubscribtionValue);            
         
         if(telegramD.IndexTelegramD != 0)
         {
            if(lastIndexD != telegramD.IndexTelegramD)
            {
               lastIndexD = telegramD.IndexTelegramD;         
               readTelegramE = true;
            }
            else
            {
               LogManager.GetSingleton().ZLog("C006A", ELF.INFO, "Doppeltes Telegram D wird ignoriert -> {0}", telegramD.IndexTelegramD);
            }
         }

         telegramD.VisuData = string.Format("{0}", telegramD.IndexTelegramD);
      }
      
      public abstract Boolean HandleTelegramE(TelegramE telegramE, Telegram telegramSend);
      public abstract void HandleTelegramA(TelegramA telegramA);
      
      
      
      public virtual Boolean Init(string datenbank, object parent, long datenClientID, string datenClientName, long anlagenID, Dictionary<long, long> nebenAnlagen, DatenClient datenClient)
      {
         this.datenbank = datenbank;
         int maxLenBezeichnung = 30;

			this.datenClient = datenClient;
         
         this.datenClientID = datenClientID;
         this.datenClientName = datenClientName;
         this.anlagenID = anlagenID;
         this.nebenAnlagen = nebenAnlagen;
         
         spsAuftraege = new Dictionary<string, Auftrag>();
         clientAuftraege = new Dictionary<string, Auftrag>();
         
         SqlRealtimeSimpleQuery query = null;
         string statement = "";
         
         try
         {
            statement = string.Format("select max(len(Bezeichnung)) as MaxLen from DC_AufträgeTIA where Datenclient_ID = {0}", datenClientID);
            query = new SqlRealtimeSimpleQuery(datenbank, statement);

            if (query.QueryResult != null)
            {
               foreach (Dictionary<string, object> prm in query.QueryResult)
               {
                  maxLenBezeichnung = DbTools.GetInt32(prm, "MaxLen");
                  break;
               }
            }
         }
         catch
         {
            LogManager.GetSingleton().ZLog("CD24E", ELF.ERROR, "Exception in Statement: {0}", statement);
         }
            

         try
         {
            statement = string.Format("select * from DC_AufträgeTIA where Datenclient_ID = {0} order by Datenclient_ID, AuftragsNummer", datenClientID);
            query = new SqlRealtimeSimpleQuery(datenbank, statement);

            if (query.QueryResult != null)
            {
               foreach (Dictionary<string, object> prm in query.QueryResult)
               {
                  Int64 auftragsID = DbTools.GetInt64(prm, "ID");
                  Int64 dcId = DbTools.GetInt64(prm, "Datenclient_ID");
                  Int64 auftragsNummer = DbTools.GetInt64(prm, "AuftragsNummer");
                  string bezeichnung = DbTools.GetString(prm, "Bezeichnung");
                  Boolean aktiv = DbTools.GetBoolean(prm, "Aktiv");
                  string tabellenName = DbTools.GetString(prm, "TabellenName");
                  string updateKriterium = DbTools.GetString(prm, "UpdateKriterium");
                  
                  //Leerzeichen verbannen
                  updateKriterium = updateKriterium.Replace(" ","");
                  
                  Int32 updateIntervall = DbTools.GetInt32(prm, "UpdateIntervall");
                  FktNr funktionsNummer = (FktNr)DbTools.GetInt64(prm, "FunktionsNummer");
                  string pseudoScript = DbTools.GetString(prm, "Pseudoscript");
                  string logTableForFN2 = "";
                  string sqlSuffix = "";
                  Boolean lockID = false;
                  
                  
                  try
                  {
                     logTableForFN2 = DbTools.GetString(prm, "LogTableForFN2");
                  }
                  catch
                  {
                     //Diese Spalte gibt es erst seit dem 01.03.2012
                     //Wenn sie nicht da ist steht die Funktion, das Daten die zur SPS gesendet werden (FN02RdRowEvtSPS) nicht zur Verfügung
                     //Der DatenClieNT läuft aber trotzdem !
                  }

                  try
                  {
                     lockID = DbTools.GetBoolean(prm, "LockID");
                  }
                  catch
                  {
                     //Diese Spalte gibt es erst seit dem 23.11.2015
                     //Wenn sie nicht da ist können keine DC_AufträgeTIA gegeneinander verriegelt werden
                     //Kurzanleitung:
                     //Alle Aufträge die autark laufen sollen, weil mögl. zwei DC-Stränge gleiche Tabellen
                     //beschreiben mit LockID = true konfigurieren.
                     //Falls die Spalte in der DC_AufträgeTIA noch fehlt als Bit anlegen !
                     //Der DatenClieNT läuft aber trotzdem !
                  }

                  if (lockID)
                  {
                     LogManager.GetSingleton().ZLog("CD24D", ELF.DEVELOPER, "Auftrag = {0} wird verriegelt ausgeführt !", auftragsNummer);
                  }
                  
                  
                  try
                  {
                     sqlSuffix = DbTools.GetString(prm, "SqlSuffix");
                  }
                  catch
                  {                     
                  }
                  
                  
                  
                  
                  object tag = null;
                  
                  try
                  {
                     tag = prm["Tag"];
                  }
                  catch
                  {
                     //Diese Spalte muß nicht da sein. Im DatenbankSetup ist sie seit dem 23.04.2012
                     //Sie wird für die Erweiterung der Pseudovariablen-Funktion benötigt.
                     //Wenn sie nicht da ist, kann dieser Teil eben nicht konfiguriert werden.
                  }
                  
                  
                  if (!DatenClient.IsClientAuftrag(funktionsNummer))
                  {
                     if (aktiv)
                     {
                        try
                        {
                           spsAuftraege[Auftrag.AuftragPrimaryKey(auftragsNummer, anlagenID)] = new Auftrag(datenbank, maxLenBezeichnung, this, auftragsID, dcId, auftragsNummer, anlagenID, bezeichnung, funktionsNummer, tabellenName, updateKriterium, updateIntervall, pseudoScript, logTableForFN2, tag, sqlSuffix, lockID);

                           LogManager.GetSingleton().ZLog("C006B", ELF.INFO, "Auftrag {0}->({1}) angelegt.", bezeichnung, Auftrag.AuftragPrimaryKey(auftragsNummer, anlagenID));
                        }
                        catch (Exception e)
                        {
                           LogManager.GetSingleton().ZLog("C006C", ELF.ERROR, "Daten.Init -> Auftrag (AuftragsNummer:AnlagenID = {0}) -> {1}", Auftrag.AuftragPrimaryKey(auftragsNummer, anlagenID), e.Message);
                           return false;
                        }
                     }
                     else
                     {
                        LogManager.GetSingleton().ZLog("C006D", ELF.INFO, "Auftrag (AuftragsNummer:AnlagenID = {0}) ist inaktiv.", Auftrag.AuftragPrimaryKey(auftragsNummer, anlagenID));
                     }
                  }
                  else
                  {
                     if (aktiv)
                     {
                        try
                        {
                           if(updateIntervall < 1)
                           {
                              LogManager.GetSingleton().ZLog("C006E", ELF.ERROR, "Daten.Init -> Beim ClientAuftrag {0} ist kein gültiges UpdateIntervall eingetragen (Tabelle DC_AufträgeTIA; Spalte UpdateIntervall). Der Auftrag konnte nicht angelegt werden.", Auftrag.AuftragPrimaryKey(auftragsNummer, anlagenID));
                              return false;
                           }
                           
                           clientAuftraege[Auftrag.AuftragPrimaryKey(auftragsNummer, anlagenID)] = new Auftrag(datenbank, maxLenBezeichnung, this, auftragsID, dcId, auftragsNummer, anlagenID, bezeichnung, funktionsNummer, tabellenName, updateKriterium, updateIntervall, pseudoScript, logTableForFN2, tag, sqlSuffix, lockID);


                           LogManager.GetSingleton().ZLog("C0070", ELF.INFO, "Auftrag {0}->({1}) angelegt.", bezeichnung, Auftrag.AuftragPrimaryKey(auftragsNummer, anlagenID));
                        }
                        catch (Exception e)
                        {
                           LogManager.GetSingleton().ZLog("C0071", ELF.ERROR, "Daten.Init -> Auftrag (AuftragsNummer:AnlagenID = {0}) -> {1}", Auftrag.AuftragPrimaryKey(auftragsNummer, anlagenID), e.Message);
                           return false;
                        }
                     }
                     else
                     {
                        LogManager.GetSingleton().ZLog("C0072", ELF.INFO, "Auftrag (AuftragsNummer:AnlagenID = {0}) ist inaktiv.", Auftrag.AuftragPrimaryKey(auftragsNummer, anlagenID));
                     }
                  }
               }
            }
         }
         catch (Exception e)
         {
            LogManager.GetSingleton().ZLog("CD24F", ELF.ERROR, "Exception in Statement: {0}", statement);
            LogManager.GetSingleton().ZLog("C0073", ELF.ERROR, "Daten.Init {0} -> Ist die Spalte Datenclient_ID in der DC_AufträgeTIA angelegt ?", e.Message);
            return false;
         }
         
         return true;
      }
    
      

      
      protected Boolean DatenLesenDB(Auftrag auftrag, TelegramE telegramE, Telegram telegramM)
      {
         object value = null;
         string logParameterString = "";
         
         Boolean datenLesenOk = true;


         try
         {
            SqlRealtimeExtendetQuery query = new SqlRealtimeExtendetQuery(datenbank, auftrag.Statement);
            SqlRealtimeExtendetExecute logQuery = null;

            if (auftrag.LoggingTable)
            {
               logQuery = new SqlRealtimeExtendetExecute(datenbank, auftrag.LogStatement);
            }

            int index = 0;

            foreach (string parameterName in auftrag.QueryParameterNames)
            {
               value = telegramE.Data[auftrag.QueryParameterAuftragsDetails[0].Id.ToString()];

               //value = Convert.ToInt64(value);
               SPS2DB(ref value, auftrag.QueryParameterAuftragsDetails[0]);
               


               index++;

               logParameterString += string.Format("\n    {0}={1}", parameterName, value);

               query.AddParameter(parameterName, value);

               if (auftrag.LoggingTable)
               {
                  logQuery.AddParameter(parameterName, value);
               }
            }

            query.SetPseudoVars(auftrag);

            if (auftrag.LoggingTable)
            {
               logQuery.SetPseudoVars(auftrag);
               logQuery.AddParameter("@LOG_DateTime", DateTime.Now);
            }


            LogManager.GetSingleton().ZLog("C0002", ELF.INFO, "Auftrag   = {0}", auftrag.Bezeichnung);
            LogManager.GetSingleton().ZLog("C0003", ELF.INFO, "Statement = {0}", auftrag.Statement);
            LogManager.GetSingleton().ZLog("C0004", ELF.INFO, "Parameter = {0}", logParameterString);

            if (datenLesenOk)
            {
               HiresStopUhr stopUhr = new HiresStopUhr();
               stopUhr.Start();

               query.Execute();

               if (query.QueryResult != null)
               {
                  if (query.QueryResult.Count < 1)
                  {
                     dataAcknoledge.SetSpsReturnCode(SpsReturnCodes.DatabaseStatementNoData);

                     LogManager.GetSingleton().ZLog("C0006", ELF.WARNING, "Daten.DatenLesenDB -> Abfrage lieferte keinen Datensatz: Auftrag = {0} Anzahl Ergebnisse = {1}", auftrag.Bezeichnung, query.QueryResult.Count);



                     datenLesenOk = false;
                  }
               }
               else
               {
                  dataAcknoledge.SetSpsReturnCode(SpsReturnCodes.DatabaseUnavailable);

                  LogManager.GetSingleton().ZLog("C0007", ELF.ERROR, "Daten.DatenLesenDB -> Abfrage lieferte kein Ergebnis: Auftrag = {0}", auftrag.Bezeichnung);
                  datenLesenOk = false;
               }
            }

            if (query.QueryResult.Count == 1)
            {
               Dictionary<string, object> prm = query.QueryResult[0];

               telegramM.RegistrationItems.Clear();
               telegramM.RegistrationValues.Clear();
               

               foreach (AuftragsDetail auftragsDetail in auftrag.AuftragDetails.Values)
               {

                  try
                  {
                     //if(auftragsDetail.TabellenSpalte == "M_I_Au_Auftr_Ka_Soll")
							//{
							//}

                     //DB2SPS Hier später den Datentyp der SPS erkennen, bzw. schon erkannt haben und entsprechend
                     //den zu schreibenden Wert casten ! Ein double in der Tabellenspalte würde sonst beim Schreiben auf einen int in der SPS 
                     //abschmieren, da alles textbasiert übertragen wird bis zu der Funktion
                     //"public string WriteValues(List<String> values, List<String> nodeIdStrings, string tag)" in der UAClientHelperAPI

							//CG: 05.01.2022 Wenn FktNr. 20 nur ein Ergebnis hat kann er hier landen statt weiter unten
							//Daher auch hier den @index@ austauschen
							string uaItem = auftragsDetail.UaItem;
							if(uaItem.Contains("@INDEX@"))
							{
								uaItem = auftragsDetail.UaItem.Replace("@INDEX@", datenClient.StartOffsetArrays.ToString());
							}

                     telegramM.RegistrationItems[auftragsDetail.Id.ToString()] = uaItem;

							prm[auftragsDetail.TabellenSpalte] = CheckNull(prm[auftragsDetail.TabellenSpalte], auftragsDetail.SqlDatentyp, uaItem, auftragsDetail.TabellenSpalte);

                     if(auftragsDetail.WertFactor != 1)
                     {
								telegramM.RegistrationValues[auftragsDetail.Id.ToString()] = ((Convert.ToDouble(prm[auftragsDetail.TabellenSpalte]) * auftragsDetail.WertFactor)).ToString();                     
                     }
                     else
                     {
                        telegramM.RegistrationValues[auftragsDetail.Id.ToString()] = prm[auftragsDetail.TabellenSpalte].ToString();                     
                     }
                  }
                  catch (Exception e)
                  {
                     throw new Exception(string.Format("Fehler bei AuftragsDetail mit Tabellenspalte = {0}: Meldung = {1}", auftragsDetail.TabellenSpalte, e.Message));
                  }
               }

               
            }
            else
            {
               //Bei Funktionsnummer 20 kann es mehrere Tupel geben.....
               
               telegramM.RegistrationItems.Clear();
               telegramM.RegistrationValues.Clear();

					//INDEX 18112020
               int rowNumber = datenClient.StartOffsetArrays;

               foreach (Dictionary<string, object> prm in query.QueryResult)
               {
                  foreach (AuftragsDetail auftragsDetail in auftrag.AuftragDetails.Values)
                  {

							

                     if (auftragsDetail.UpdateKriterium)
                     {
                        //Braucht man nicht zurück schreiben....
                        continue;
                     }

                     try
                     {
                        telegramM.RegistrationItems[string.Format("{0}_{1}", auftragsDetail.Id.ToString(), rowNumber)] = auftragsDetail.UaItem.Replace("@INDEX@", rowNumber.ToString());

								if(auftragsDetail.WertFactor != 1)
                        {
                           telegramM.RegistrationValues[string.Format("{0}_{1}", auftragsDetail.Id.ToString(), rowNumber)] = (((double)prm[auftragsDetail.TabellenSpalte] * auftragsDetail.WertFactor)).ToString();                     
                        }
                        else
                        {
                           telegramM.RegistrationValues[string.Format("{0}_{1}", auftragsDetail.Id.ToString(), rowNumber)] = prm[auftragsDetail.TabellenSpalte].ToString();
                        }


                        
                     }
                     catch (Exception e)
                     {
                        throw new Exception(string.Format("Fehler bei AuftragsDetail mit Tabellenspalte = {0}: Meldung = {1}", auftragsDetail.TabellenSpalte, e.Message));
                     }
                  }

                  

                  rowNumber++;
               }
            }
         }
         catch(Exception e12)
         {            
				LogManager.GetSingleton().ZLog("D0001", ELF.ERROR, "Auftrag = {0} -> {1} ->Statement = {2}", auftrag.Bezeichnung, e12.Message, auftrag.Statement);
         }

         
         return datenLesenOk;
      }

		
		private object CheckNull(object obj, string sqlDatentyp, string uaItem, string tabellenSpalte)
		{
			if(obj == System.DBNull.Value)
			{
				//CG 25.11.2021
				//Im alten DatenClient war NULL in den Tabellenspalten wohl auch zulässig
				//Daher hier einfach NULL = Ersatzwert
				switch(sqlDatentyp.ToLower())
				{
					case "bigint":
					case "decimal":
					case "float":
					case "int":
					case "numeric":
					case "real":
					case "smallint":
					case "tinyint":
						LogManager.GetSingleton().ZLog("D000A", ELF.INFO, "Ersatzwert für Variable '{0}' = {1} da Tabellenspalte {2} NULL ist", uaItem, "0", tabellenSpalte);
					return 0;

					case "bit": 
						LogManager.GetSingleton().ZLog("D000D", ELF.INFO, "Ersatzwert für Variable '{0}' = {1} da Tabellenspalte {2} NULL ist", uaItem, "false", tabellenSpalte);
					return "false";

					case "char":
					case "nchar":					
					case "nvarchar":
					case "sysname":
					case "text":
					case "varchar": 
						LogManager.GetSingleton().ZLog("D000B", ELF.INFO, "Ersatzwert für Variable '{0}' = {1} da Tabellenspalte {2} NULL ist", uaItem, "Leerstring", tabellenSpalte);
					return "";

					case "datetime": 
						LogManager.GetSingleton().ZLog("D000C", ELF.INFO, "Ersatzwert für Variable '{0}' = {1} da Tabellenspalte {2} NULL ist", uaItem, "01.01.1900 00:00:00", tabellenSpalte);
					return "01.01.1900 00:00:00";
					
					
				}
				
				return obj;
			}
			else
			{
				//unverändert wenn nicht NULL in der Datenbank steht
				return obj;
			}
		}

		private void SPS2DB(ref object value, AuftragsDetail auftragsDetail)
      {
         if(value.ToString().Contains("∞") && value.GetType().FullName == "System.Single")
         {   
            value = (System.Single) 1000000000;
         }
         else if(value.ToString().Contains("∞") && value.GetType().FullName == "System.Double")
         {   
            value = (System.Double) 1000000000;
         }

         //Sonderfälle berücksichtigen bei denen der eingelesene Datentyp des OPC-UA-Servers
         //nicht mit dem SQL-Datentyp direkt kompatibel sind
         if (value.GetType().FullName == "System.Byte[]")
         {
            //Je nach OPC-Server-UA (TANI oder Direkt SPS) kommt einmal schon ein C#-DateTime vom Server, 
            //aber auch mal ein System.Byte[] mit 8 Einträgen. Wenn dies so ist muss erst 
            //ein Datum aus den 8 Bytes gebastelt werden
            if (auftragsDetail.SqlDatentyp == "DATETIME")
            {
               string strDateTime = string.Format("20{0:X2}.{1:X2}.{2:X2} {3:X2}:{4:X2}:{5:X2}", ((System.Byte[])value)[0], ((System.Byte[])value)[1], ((System.Byte[])value)[2], ((System.Byte[])value)[3], ((System.Byte[])value)[4], ((System.Byte[])value)[5]);
               DateTime dateTime = DateTime.ParseExact(strDateTime, "yyyy.MM.dd HH:mm:ss", null);
               value = dateTime;
            }
         }

         if(auftragsDetail.Id == 1123025)
         {
            
         }

         //yyyyyyyyyyyy
         if (auftragsDetail.SqlDatentyp == "BIGINT")
         {
            //UINT-Datentypen sind seltsamerweise nicht mit SQL-Vorzeichenbehafteten Datentypen kompatibel
            //Im konkreten Fall hat eine SqlAbfrage mit Parameter UInt16 nicht zu bigint gepasst.
            value = Convert.ToInt64(value);
         }
         else if (auftragsDetail.SqlDatentyp == "INT")
         {
            //UINT-Datentypen sind seltsamerweise nicht mit SQL-Vorzeichenbehafteten Datentypen kompatibel
            //Im konkreten Fall hat eine SqlAbfrage mit Parameter UInt16 nicht zu bigint gepasst.
            value = Convert.ToInt32(value);
         }


         if (auftragsDetail.WertFactor != 1)
         {
            try
            {
               value = ((Convert.ToDouble(value)) / ((double)auftragsDetail.WertFactor));
            }
            catch (Exception e)
            {
               throw new Exception(string.Format("AuftragsDetail {0} hat einen Faktor aber der Wert {1} ist nicht als Dezimalzahl interpretierbar. {2}", auftragsDetail.Id, value, e.Message));
            }
         }
      }

      private Boolean DatenSchreibenDB(Auftrag auftrag, Dictionary<string, object> data)
      {
         Boolean ok = true;         
         string logParameterString = "";


			if(data == null)
			{
				
			}
         
         try
         {
            
            SqlRealtimeExtendetExecute sqlRealtimeExtendetExecute = new SqlRealtimeExtendetExecute(datenbank, auftrag.Statement);
            sqlRealtimeExtendetExecute.SetPseudoVars(auftrag);
            
            string parameterName = "";
            object value = null;
            
            int paramCounter = 0;

            foreach (AuftragsDetail auftragsDetail in auftrag.AuftragDetails.Values)
            {
               paramCounter++;

					

               foreach (string parameter in auftrag.Parameter)
               {
                  parameterName = "";
                  value = null;

                  switch (parameter)
                  {
                     case "AuftragsDetail_ID":
                        parameterName = string.Format("@{0}{1}", parameter, paramCounter);
                        value = auftragsDetail.Id;
                        break;
                     case "Wert":
                        parameterName = string.Format("@{0}{1}", parameter, paramCounter);

                        value = data[auftragsDetail.Id.ToString()];

                        try
                        {
                           if(auftrag.AuftragsNummer == 102 && parameterName == "@Wert101")
									{
									}

                           SPS2DB(ref value, auftragsDetail);
                        }
                        catch(Exception eInner)
                        {
                           throw eInner;
                        }

                        if (value == System.DBNull.Value)
                        {
                           dataAcknoledge.SetSpsReturnCode(SpsReturnCodes.Sps2DbConversionError);
                           LogManager.GetSingleton().ZLog("C000B", ELF.ERROR, "Daten.DatenEintragen für Auftrag ({0}) fehlgeschlagen. SPS2DBValue lieferte keinen Wert zurück.", auftrag.Bezeichnung);
                           return false;
                        }
                        break;
                     default:
                        LogManager.GetSingleton().ZLog("C000C", ELF.ERROR, "Daten.DatenEintragen -> Parameter konnte nicht zugeordnet werden. Daten werden nicht eingetragen. Auftrag = {0}", auftrag.Bezeichnung);
                        return false;
                  }

                  try
                  {
                     logParameterString += BuildSqlVariable(parameterName, value, value.GetType()); 
                     sqlRealtimeExtendetExecute.AddParameter(parameterName, value);
                  }
                  catch
                  {
                  }

               }
            }

            LogManager.GetSingleton().ZLog("C000E", ELF.INFO, "Auftrag  ={0}", auftrag.Bezeichnung);
            LogManager.GetSingleton().ZLog("C000F", ELF.INFO, "Statement={0}", auftrag.Statement);
            LogManager.GetSingleton().ZLog("C0010", ELF.INFO, "Parameter={0}", logParameterString);
            
            sqlRealtimeExtendetExecute.ExecuteNonQuery();
         }
         catch (Exception e)
         {
            dataAcknoledge.SetSpsReturnCode(SpsReturnCodes.UnspecifiedError);
            
            ok = false;

            if(e.Message.ToLower().Contains("sqlserveragent"))
            {
               LogManager.GetSingleton().ZLog("CFFF3", ELF.ERROR, "DatenEintragen -> Auftrag = {0} -> {1} -> {2}", auftrag.Bezeichnung, e.Message, "Es liegt vermutlich an einem Trigger, der einen Job auslöst. Dazu muss der SQLServerAgent laufen !");
            }
            else
            {
               LogManager.GetSingleton().ZLog("C0013", ELF.ERROR, "DatenEintragen -> Auftrag = {0} -> {1}", auftrag.Bezeichnung, e.Message);
            }
         }
         
         return ok;
      }

      private string BuildSqlVariable(string parameterName, object value, Type type)
      {
         string sqlDatenTyp = "";
         string potentialHochkomma = "";
         string textValue = "";
         string ret = "";

         try
         {         
            switch(type.FullName)
            {
               case "System.Int64": 
                  sqlDatenTyp = "bigint"; 
                  textValue = Convert.ToInt64(value).ToString();
               break;
               case "System.Int32": 
                  sqlDatenTyp = "int"; 
                  textValue = Convert.ToInt32(value).ToString();
                  break;
               case "System.Single": 
                  sqlDatenTyp = "float"; 
                  textValue = Convert.ToDouble(value).ToString();
                  break;
						case "System.Double": 
                  sqlDatenTyp = "float"; 
                  textValue = Convert.ToDouble(value).ToString();
                  break;
               case "System.String": 
                  potentialHochkomma = "'";
                  textValue = Convert.ToString(value);
                  sqlDatenTyp = string.Format("nvarchar({0})", textValue.Length); 
                  break;
						case "System.Boolean": 
						textValue = "true";
						if(Convert.ToBoolean(value) == false)
						{
							textValue = "false";
						}
                  sqlDatenTyp = string.Format("nvarchar({0})", textValue.Length); 
                  break;
               default : sqlDatenTyp = ""; break;
            }

            ret = string.Format("declare {0} as {2}; set {0}={1}{3}{1};", parameterName, potentialHochkomma, sqlDatenTyp, textValue);         
            ret = ret + Environment.NewLine;
         
            
         }
         catch
         {
         }

         return ret;
      }

      protected Boolean SafeDatenSchreibenDB(Auftrag auftrag, Dictionary<string, object> data)
      {
         Boolean result = false;

			if(data == null)
			{

				LogManager.GetSingleton().ZLog("CFFF8", ELF.ERROR, "Auftrag ({0}) hat keine Daten. SPS nicht erreichbar oder mind. ein Items nicht lesbar.", auftrag.Bezeichnung);

				dataAcknoledge.SetSpsReturnCode(SpsReturnCodes.Sensorfehler);

				return false;
			}

         if(auftrag.LockID)
         {
            LogManager.GetSingleton().ZLog("CD24C", ELF.DEVELOPER, "Auftrag ({0}) wird wegen DC_Auftraege LockID verriegelt !", auftrag.Bezeichnung);

            HiresStopUhr stopUhr = new HiresStopUhr();
            stopUhr.Start();
            
            lock (Auftrag.LockObject)
            {
               stopUhr.Stop();

               if(stopUhr.PeriodMilliSeconds > 1000)
               {
                  LogManager.GetSingleton().ZLog("CD251", ELF.WARNING, "Auftrag ({0}) läuft mit Lock ! Wartezeit war {1} ms.", auftrag.Bezeichnung, stopUhr.PeriodMilliSeconds);
               }
               else
               {
                  LogManager.GetSingleton().ZLog("CD254", ELF.DEVELOPER, "Auftrag ({0}) läuft mit Lock ! Wartezeit war {1} ms.", auftrag.Bezeichnung, stopUhr.PeriodMilliSeconds);
               }
               
               
               
               result = DatenSchreibenDB(auftrag, data);
            }
         }
         else
         {
            result = DatenSchreibenDB(auftrag, data);
         }


         return result;
      }

      

      protected Boolean SafeDatenSchreibenDB(Auftrag auftrag, SqlConnection dataConnection, AuftragsDetail auftragsDetail, object value)
      {
         Boolean result = false;
         
         if (auftrag.LockID)
         {
            LogManager.GetSingleton().ZLog("CD24A", ELF.DEVELOPER, "Auftrag ({0}) wird wegen DC_Auftraege LockID verriegelt !", auftrag.Bezeichnung);

            HiresStopUhr stopUhr = new HiresStopUhr();
            stopUhr.Start();
            
            lock (Auftrag.LockObject)
            {
               stopUhr.Stop();

               if (stopUhr.PeriodMilliSeconds > 1000)
               {
                  LogManager.GetSingleton().ZLog("CD24B", ELF.WARNING, "Auftrag ({0}) läuft mit Lock ! Wartezeit war {1} ms.", auftrag.Bezeichnung, stopUhr.PeriodMilliSeconds);
               }
               else
               {
                  LogManager.GetSingleton().ZLog("CD255", ELF.DEVELOPER, "Auftrag ({0}) läuft mit Lock ! Wartezeit war {1} ms.", auftrag.Bezeichnung, stopUhr.PeriodMilliSeconds);
               }
               
               
               result = DatenSchreibenDB(auftrag, dataConnection, auftragsDetail, value);
            }
         }
         else
         {
            result = DatenSchreibenDB(auftrag, dataConnection, auftragsDetail, value);
         }


         return result;
      }

      private Boolean DatenSchreibenDB(Auftrag auftrag, SqlConnection dataConnection, AuftragsDetail auftragsDetail, object value)
      {
         //Funktionsnummer 11 wird aus Performancegründen extra behandelt.
         //Es werden nur Werte geschrieben (upgedatet) die sich geändert haben.
         
         try
         {
            SqlRealtimeExtendetExecute sqlRealtimeExtendetExecute = new SqlRealtimeExtendetExecute(dataConnection, auftragsDetail.Statement);
            

            if (value == System.DBNull.Value)
            {
               dataAcknoledge.SetSpsReturnCode(SpsReturnCodes.DatabaseStatementNoData);

               LogManager.GetSingleton().ZLog("C0014", ELF.ERROR, "Daten.DatenEintragen für Auftrag ({0}) fehlgeschlagen. SPS2DBValue lieferte keinen Wert zurück.", auftrag.Bezeichnung);
               return false;
            }

            if (!value.Equals(auftragsDetail.LastValue))
            {
               sqlRealtimeExtendetExecute.SetPseudoVars(auftrag);

               sqlRealtimeExtendetExecute.AddParameter("@AuftragsDetail_ID", auftragsDetail.Id);


               if (auftragsDetail.SqlDatentyp == "BIGINT")
               {
                  //UINT-Datentypen sind seltsamerweise nicht mit SQL-Vorzeichenbehafteten Datentypen kompatibel
                  //Im konkreten Fall hat eine SqlAbfrage mit Parameter UInt16 nicht zu bigint gepasst.
                  value = Convert.ToInt64(value);
               }
               else if (auftragsDetail.SqlDatentyp == "INT")
               {
                  //UINT-Datentypen sind seltsamerweise nicht mit SQL-Vorzeichenbehafteten Datentypen kompatibel
                  //Im konkreten Fall hat eine SqlAbfrage mit Parameter UInt16 nicht zu bigint gepasst.
                  value = Convert.ToInt32(value);
               }
               else if(value.GetType().FullName == "System.UInt16")
					{
						//Datenbank kann keine Werte ohne Vorzeichen
						value = Convert.ToInt16(value);
					}
               else if(value.GetType().FullName == "System.UInt32")
					{
						//Datenbank kann keine Werte ohne Vorzeichen
						value = Convert.ToInt32(value);
					}
               else if(value.GetType().FullName == "System.UInt64")
					{
						//Datenbank kann keine Werte ohne Vorzeichen
						value = Convert.ToInt64(value);
					}

               sqlRealtimeExtendetExecute.AddParameter("@Wert", value);
               
               sqlRealtimeExtendetExecute.ExecuteNonQueryRecycleSqlConnection();

               auftragsDetail.LastValue = value;
            }
         }
         catch(Exception e)
         {
            throw new Exception(string.Format("{0} -> Statement = {1} | @AuftragsDetail_ID = {2} @Wert = |{3}|", e.Message, auftragsDetail.Statement, auftragsDetail.Id, value));
         }

         return true;
      }

      protected void SetPseudoVars(SqlCommand sqlCommand, Auftrag auftrag)
      {
         foreach (string pseudoVariable in auftrag.PseudoScript.VariablenListe.Values)
         {
            string parameterName = pseudoVariable;

            object value = null;

            if (pseudoVariable == "@DC_UTC_HIRES")
            {
               //100 Nanosekunden seit dem 01.01.1601
               value = DateTime.UtcNow.ToFileTimeUtc();
            }
            else if (pseudoVariable == "@DC_UTC_DATETIME")
            {
               value = DateTime.UtcNow;               
            }
            else if (pseudoVariable == "@DC_LOCAL_DATETIME")
            {
               value = DateTime.Now;
            }
            else if (pseudoVariable == "@ANLAGEN_ID")
            {
               value = auftrag.AnlagenID;
            }
            else if (pseudoVariable == "@AUFTRAGS_NR")
            {
               value = auftrag.AuftragsNummer;
            }
            else if (pseudoVariable == "@TAG")
            {
               value = auftrag.Tag;
            }
            
            if(!sqlCommand.Parameters.Contains(parameterName))
            {
               sqlCommand.Parameters.AddWithValue(parameterName, value);
            }
            else
            {
               //Variable kommt im PseudoScript mehrfach vor, aber soll in verschiedene Spalten eingetragen 
               //werden. Das ist kein Problem. Nur der Parameter darf natürlich nur einmal gesetzt werden !
            }
         }
      }
   }
}
