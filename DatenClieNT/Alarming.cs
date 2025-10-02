using System;
using System.Collections.Generic;
using System.Text;

namespace DatenClieNT
{
   abstract class Alarming
   {
      private Boolean aktivAlarming;
		protected Boolean programmNummerAlt = true; 
      private StoerBerNurAnzeige stoerBerNurAnz = null;
      protected long datenClientID;
      private long anlagenID;
      protected Dictionary<long, long> nebenAnlagen;
      protected string name;
      protected string database;
      
      public string Name
      {
         get { return name; }
      }

      //Status-Zähler
      private int alarmCounter = 0;

      protected long AnlagenID
      {
         get { return anlagenID; }
      }

      protected string NebenAnlagenForSql
      {
         get
         {
            string nebenAnlagenIds = "";
            string trennzeichen = "";

            foreach (long id in nebenAnlagen.Keys)
            {
               nebenAnlagenIds += trennzeichen;
               nebenAnlagenIds += string.Format("{0}", id);

               trennzeichen = ",";
            }
            
            return nebenAnlagenIds;
         }
      }

      public bool AktivAlarming
      {
         get { return aktivAlarming; }
      }

      protected bool StoerBerNurAnz(int bereich)
      {
         if(stoerBerNurAnz == null)
         {
            return false;
         }
         
         return stoerBerNurAnz.StoerNurAnzeige(bereich);
      }

      public virtual void HandleTelegramS(Telegram telegram, out bool readTelegramT)
      {
         readTelegramT = false;
         
         alarmCounter++;
      }
      
      public abstract void HandleTelegramT(Telegram telegram, out bool quit, out int quitValue);

      public virtual Boolean Init(string database, DatenClient datenClient, long datenClientID, long anlagenID, Dictionary<long, long> nebenAnlagen)
      {
         this.database = database;
         this.datenClientID = datenClientID;
         this.anlagenID = anlagenID;
         this.nebenAnlagen = nebenAnlagen;
         this.name = string.Format("Alarming {0}", datenClient.Name);


			

         try
         {
            SqlRealtimeSimpleQuery query = new SqlRealtimeSimpleQuery(datenClient.Database, string.Format("select * from DC_DatenclientsTIA where id = {0}", datenClientID));

            if (query.QueryResult != null)
            {
               foreach (Dictionary<string, object> prm in query.QueryResult)
               {
                  try
                  {
                     string nurAnz = Tools.GetString(prm, "StoerBerNurAnzeige");
                     aktivAlarming = Tools.GetBoolean(prm, "AktivAlarming");
                     
                     stoerBerNurAnz = new StoerBerNurAnzeige(nurAnz);
                  }
                  catch (Exception e)
                  {
                     LogManager.GetSingleton().ZLog("C0021", ELF.ERROR, "Error in Alarming.Init() {1}-> datenClientId={0}", e.Message, datenClientID);
                     return false;
                  }
               }
            }
         }
         catch (Exception e)
         {
            LogManager.GetSingleton().ZLog("C0022", ELF.ERROR, "Error in Alarming.Init() -> {0}", e.Message);
            return false;
         }

         return true;
      }


      private Boolean refreshAlarmTrimSpacesAvailable = false;
      private Boolean alarmTrimSpaces = true;
      
      private void CheckTrimSpacesProperty()
      {
         //CG: 11.01.2017
         //Hier wird getestet ob der DC die Operanden der Alarme trimmen soll oder nicht
         //Bei der IMC läuft der neue DC aber eben mit dieser (alten) Einstellung -> nicht trimmen !
         //GetParameter kann man hier nicht nehmen, da sie noch nicht geholt wurden zu diesem Zeitpunkt
         //Damit nicht jedesmal auf die DB zugegriffen wird, nur einmal das Flag holen und dann merken.
         //Nachträgliche Änderungen sind nicht mehr möglich wie bei den anderen Parametern
         
         try
         {
            if (!refreshAlarmTrimSpacesAvailable)
            {
               if (ConfigManager.GetSingleton().RefreshAlarmTrimSpaces())
               {
                  string paramAlarmTrimSpaces = ConfigManager.GetSingleton().GetParameter("ALARM_TRIM_SPACES", string.Format("{0}", 1));

                  if (paramAlarmTrimSpaces == "0")
                  {
                     alarmTrimSpaces = false;
                  }
                  else
                  {
                     alarmTrimSpaces = true;
                  }
                  refreshAlarmTrimSpacesAvailable = true;
               }
               else
               {

               }
            }
         }
         catch(Exception e)
         {
            LogManager.GetSingleton().ZLog("CD256", ELF.ERROR, "CheckTrimSpacesProperty fehlgeschlagen. Annahme->Spaces bei Alrmen trimmen ! -> Exception={0}", e.Message);  
            refreshAlarmTrimSpacesAvailable = true;
            alarmTrimSpaces = true;
         }


			string programmNrAlt = ConfigManager.GetSingleton().GetParameter("Programmnummer_ALT", "0");

			if(programmNrAlt == "1")
			{
				programmNummerAlt = true;
			}
			else
			{
				programmNummerAlt = false;
			}
      }
      
      protected virtual bool AktuellenAlarmEintragen(Boolean nurAnz, long anlagenID, string teleArt, Boolean teleNichtSpeichern, Boolean teleNichtAnzeigen, int alarmBereichId, string programNummer, short schritt, string operation, string operand, int alarmStatusId, DateTime currentDate)
      {
         CheckTrimSpacesProperty();
      
         try
         {
            if(alarmTrimSpaces)
            {
               //Standardmäßig sollen Operanden ohne Spaces eingetragen werden.
               operand = operand.Replace(" ", "");
            }
         
         
            string query = "";
            int hmAktualFlag = 1;

            if (nurAnz) 
            {
               hmAktualFlag = 2;
            }

				if(programmNummerAlt)
				{
					query = string.Format("update ST_AlarmAktuell set ActualFlag = {0} where Operand = '{1}' and AlarmBereich_ID = {2} and Anlagen_ID = {3} and Programm_Nr = '{4}' and Schritt = {5}", hmAktualFlag, operand, alarmBereichId, anlagenID, programNummer, schritt);
				}
				else
				{
					query = string.Format("update ST_AlarmAktuell set ActualFlag = {0} where Operand = '{1}' and AlarmBereich_ID = {2} and Anlagen_ID = {3} and ProgrammNummer = '{4}' and Schritt = {5}", hmAktualFlag, operand, alarmBereichId, anlagenID, programNummer, schritt);
				}

            try
            {
               SqlRealtimeSimpleExecute exec = new SqlRealtimeSimpleExecute(database, query);
            }
            catch (Exception e)
            {
               LogManager.GetSingleton().ZLog("C0023", ELF.ERROR, "AktuellenAlarmEintragen1 -> {0}\n{1}", e.Message, query);

					if(e.Message.Contains("ProgrammNummer"))
					{
						LogManager.GetSingleton().ZLog("D0015", ELF.ERROR, "In der Tabelle DC_Parameter die Einträge Programmnummer_ALT und StartoffsetArrays machen und jeweils mit 1 besetzen !");
					}

               return false;
            }

				if(programmNummerAlt)
				{					
					query = string.Format("select ActualFlag from ST_AlarmAktuell where Operand = '{0}' and AlarmBereich_ID = {1} and Anlagen_ID = {2} and Programm_Nr = '{3}' and Schritt = {4}", operand, alarmBereichId, anlagenID, programNummer, schritt);
				}
				else
				{
					query = string.Format("select ActualFlag from ST_AlarmAktuell where Operand = '{0}' and AlarmBereich_ID = {1} and Anlagen_ID = {2} and ProgrammNummer = '{3}' and Schritt = {4}", operand, alarmBereichId, anlagenID, programNummer, schritt);
				}

            SqlRealtimeSimpleQuery sqlQuery = new SqlRealtimeSimpleQuery(database, query);

            Boolean alarmExists = true;
            if (sqlQuery.QueryResult != null)
            {
               LogManager.GetSingleton().ZLog("C0025", ELF.INFO, "Alarm noch vorhanden = {0}", sqlQuery.QueryResult.Count);
               
               if (sqlQuery.QueryResult.Count == 0)
               {
                  alarmExists = false;
               }
            }


            if (!alarmExists)
            {
               string datum = string.Format(" CONVERT(datetime, '{0}', 121)", currentDate.ToString("yyyyMMdd HH:mm:ss"));

             

               if(programmNummerAlt)
					{					
						query = string.Format("insert into ST_AlarmAktuell (TS_Beginn, Anlagen_ID, AlarmBereich_ID, Programm_Nr, Schritt, Operation, Operand, AlarmStatus_ID, ActualFlag) ");
						query += string.Format(" values ({0},{1},{2},'{3}',{4},'{5}','{6}',{7},{8})", datum, anlagenID, alarmBereichId, programNummer, schritt, operation, operand, alarmStatusId, hmAktualFlag);
					}
					else
					{
						query = string.Format("insert into ST_AlarmAktuell (TS_Beginn, Anlagen_ID, AlarmBereich_ID, ProgrammNummer, Schritt, Operation, Operand, AlarmStatus_ID, ActualFlag, Art, NichtSpeichern, NichtAnzeigen) ");
						query += string.Format(" values ({0},{1},{2},'{3}',{4},'{5}','{6}',{7},{8},'{9}',{10},{11})", datum, anlagenID, alarmBereichId, programNummer, schritt, operation, operand, alarmStatusId, hmAktualFlag, teleArt, Convert.ToInt16(teleNichtSpeichern), Convert.ToInt16(teleNichtAnzeigen));
					}


               //LogManager.GetSingleton().ZLog("C0026", ELF.INFO, "Alarm Eintr.->INSERT = {0}", query);

               try
               {
                  SqlRealtimeSimpleExecute exec = new SqlRealtimeSimpleExecute(database, query);
               }
               catch (Exception e)
               {
                  LogManager.GetSingleton().ZLog("C0027", ELF.ERROR, "Alarm Eintr.#2#->{0}\n{1}", e.Message, query);

						if(e.Message.Contains("varchar"))
						{
							LogManager.GetSingleton().ZLog("D0002", ELF.ERROR, "Das Item BAU_FC (DC_DatenclientsTIA_Items) darf nicht konfiguriert sein, wenn es Alarme im alten Muster geben soll. Nur bei Thyssen gibt es bis jetzt einen Bau_FC !");
						}
                  return false;
               }
            }
         }
         catch (Exception e)
         {
            LogManager.GetSingleton().ZLog("C0028", ELF.ERROR, "Alarm Eintr.#3#->{0}", e.Message);
            return false;
         }

         return true;
      }
   }
}
