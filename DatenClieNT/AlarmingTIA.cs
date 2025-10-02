using System;
using System.Collections.Generic;
using System.Text;

namespace DatenClieNT
{
   class AlarmingTIA : Alarming
   {
      private int spsStoerCounterTeleS = -1;
      private long anzahlStoerungenSeitStart = 0;
      private int stoerQuittung = 0;
		private DatenClient datenClient = null;

      public override Boolean Init(string database, DatenClient datenClient, long datenClientId, long anlagenID, Dictionary<long, long> nebenAnlagen)
      {
			this.datenClient = datenClient;
         return base.Init(database, datenClient, datenClientId, anlagenID, nebenAnlagen);
         
      }

      public override void HandleTelegramS(Telegram telegram, out bool readTelegramT)
      {
         //Eine Störungsveränderung ist eingetreten. Also die Daten dazu anfordern (Telegram T)

         readTelegramT = false;

         int spsStoerCounter = -1;
         
         try
         {
            spsStoerCounter = Convert.ToInt32(telegram.SubscribtionValue);


            if (spsStoerCounterTeleS != spsStoerCounter)
            {
               anzahlStoerungenSeitStart++;
               spsStoerCounterTeleS = spsStoerCounter;
					((TelegramS)telegram).IndexTelegramS = spsStoerCounter;

               base.HandleTelegramS(telegram, out readTelegramT);

               readTelegramT = true;
            }

            telegram.VisuData = string.Format("{0}", spsStoerCounter);
         }
         catch (Exception)
         {
            LogManager.GetSingleton().ZLog("C003D", ELF.ERROR, "AlarmingS7.HandleTelegramS -> spsStoerCounter konnte nicht gewandelt werden. -> TelegramBuffer = {0}", telegram.SubscribtionValue);
         }
      }


      private string CreateOperand(string paraTyp, string dbNr, string param)
      {
         string operand = "";
         string parameter = "";
         string bit = "";

         try
         {
            string paramAsBCD = Convert.ToString(Convert.ToUInt32(param), 16);

            bit = string.Format(paramAsBCD.Substring(paramAsBCD.Length - 1), 1);
            parameter = string.Format(paramAsBCD.Substring(0, paramAsBCD.Length - 1));

            if(parameter.Length == 0)
            {
               //----------------------------v--
               //Passiert z.B. bei DB950.DBX0.8
               //Die Zahl nach dem X ist Hex-Codiert (Offset + Bitposition)
               //Eine führende 0 wird aber nicht zurückgewandelt, als String "0", wenn sie als integer über den OPC-Server kommt
               //Daher wird die 0 als Sonderfall hardcodiert.
               parameter = "0";
            }

            if (paraTyp == "D")
            {
               operand = string.Format("DB{0}.DBX{1}.{2}", dbNr, parameter, bit);

               if(operand.Contains("950"))
               {
               }
            }
            else
            {
               operand = string.Format("{0}{1}.{2}", paraTyp, parameter, bit);
            }

            LogManager.GetSingleton().ZLog("CD31E", ELF.DEVELOPER, "AlarmingTIA.CreateOperand -> Operand-Bildung erfolgreich -> {0}", operand);
         }
         catch
         {
            LogManager.GetSingleton().ZLog("CD31D", ELF.ERROR, "AlarmingTIA.CreateOperand -> Operand-Bildung fehlgeschlagen (paraTyp={0} dbNr={1}, param={2}", paraTyp, dbNr, param);
         }

         return operand;
      }

      private string CreateDatenbankEintrag(string programmNummer, Boolean bau_FC)
      {

         if(bau_FC)
         {
            return string.Format("FC{0}", programmNummer);
         }

         return string.Format("FB{0}", programmNummer);
      }

      
      public override void HandleTelegramT(Telegram telegram, out bool quit, out int quitValue)
      {
         quitValue = 0;
         quit = false;
         HiresStopUhr stopUhr = new HiresStopUhr();

         telegram.VisuData = string.Format("T (Störkennung={0})", (((TelegramT)telegram).IndexTelegramS).ToString());

         
         if (!AktuellenAlarmAustragen())
         {
            LogManager.GetSingleton().ZLog("C0041", ELF.ERROR, "Austragen eines Alarms fehlgeschlagen ! Weitere Auswertung abgebrochen ! DatenClient = {0} Telegram T = {1}", datenClientID, "");
            return;
         }
            
         try
         {
            int anzahl = Convert.ToInt32(telegram.Data["T.Anzahl_Störeinträge"]);

				int von = datenClient.StartOffsetArrays;
				int bis = anzahl + datenClient.StartOffsetArrays;
				
            //INDEX 18112020
            for (int alarmIndex = von; alarmIndex < bis; alarmIndex++)
            {
               byte teleAnlagenID = 0;
               byte teleBereichStream = 0;
               string teleProgramNummer = "";
               byte teleSchritt = 0;
               char teleOperation = (char)0;
               int teleBedingung = 0;
               string teleOperand = "";
               string teleParaTyp = "";
               string teleDBNr = "";
               string teleParam = "";
               string teleArt = "";
               Boolean teleNichtSpeichern = false;
               Boolean teleNichtAnzeigen = false;
               char vkTyp = ' ';
					string keyOperation;
              

               //AnlagenID des Alarms holen.....
               string keyAnlagen_ID = string.Format("T.{0}_{1}", "Anlagen_ID", alarmIndex.ToString().PadLeft(3, '0'));

					try
					{
						teleAnlagenID = Convert.ToByte(telegram.Data[keyAnlagen_ID]);
					}
					catch(Exception exc)
					{
						if(exc.HResult == (-2146232969))
						{
							LogManager.GetSingleton().ZLog("D0013", ELF.ERROR, "Es stehen mehr Alarme an als verarbeitet werden können -> {0}", anzahl);
							break;
						}


						throw new Exception(string.Format("HandleTelegramT POS 1: {0}", exc.Message));
					}

               if (!nebenAnlagen.ContainsKey((long)teleAnlagenID))
               {
                  if (((long)teleAnlagenID) == 0)
                  {
                     LogManager.GetSingleton().ZLog("CD324", ELF.INFO, "Es wurden mehr Alarme gelesen als aktuell noch anstehen ! Liegt am getrennten Lesen von Anzahl und den eigentlichen Alarmen. Kein Fehler ! Index = {0} von Index = {1} bis Index = {2}", alarmIndex, von, bis);
                     //Kann bei GAT und Dietermann noch passieren
							//Bei/ab Thyssen wartet die SPS nach einer Alarm-Index-Änderung auf eine Quittierung der SPS
							//D.h. Alarme können sich im Ausgangspuffer erst wieder ändern, wenn der DC quittiert hat
							//Das sollte bei Dietermann und GAT auch noch eingebaut werden
							break;
                  }
                  else
                  {
                     //Das nebenAnlagen-Dictionary enthält auch die Hauptanlage
                     LogManager.GetSingleton().ZLog("C0042", ELF.ERROR, "HandleTelegramT -> Die AnlagenID im Telegram T ({0}) passt nicht zu der konfigurierten Anlage ({1}) und auch nicht zu den konfigurierten Nebenanlagen !", teleAnlagenID, AnlagenID);
                     LogManager.GetSingleton().ZLog("C0043", ELF.INFO, "HandleTelegramT -> Wenn dieser Alarm trotzdem eingetragen werden soll, muß er in der Tabelle Org_NebenAnlagen mit einer Hauptanlage verknüpft werden !");

                     continue;
                  }
               }
               else
               {
                  LogManager.GetSingleton().ZLog("D0014", ELF.TELE, "ALARM_INDEX = {0}", alarmIndex);
               }

               try
					{
               
						string keyBereich = string.Format("T.{0}_{1}", "Bereich", alarmIndex.ToString().PadLeft(3, '0'));
						teleBereichStream = Convert.ToByte(telegram.Data[keyBereich]);

						string keyProgrammNr = string.Format("T.{0}_{1}", "Prog_Nr", alarmIndex.ToString().PadLeft(3, '0'));
						teleProgramNummer = Convert.ToString(telegram.Data[keyProgrammNr]);

						
						string keybau_FC = string.Format("T.{0}_{1}", "Bau_FC", alarmIndex.ToString().PadLeft(3, '0'));

						if(!datenClient.StandardAlarming)
						{
							//Also muss es das Thyssenprojekt sein. 
							//Das einzige mit BauFC-Unterscheidung

							//if(telegram.Data.ContainsKey(keybau_FC))
							//Vorher abfragen, damit es kompatibel bleibt mit GAT und LagerV in Limburg und Dietermann in Viersen
							//if(!programmNummerAlt) //Ist hier noch nicht eingelesen aus der Datenbank !
							{
								Boolean bau_FC = Convert.ToBoolean(telegram.Data[keybau_FC]);
								teleProgramNummer = CreateDatenbankEintrag(teleProgramNummer, bau_FC);
							}
						}
						else
						{
							//Unbekannt da nicht von der SPS übertragen, weil nicht konfiguriert in der Tabelle DC_DatenClientsTIA_Items
							//teleProgramNummer = "NA";
						}

               
						//Art_Hinweis / Art_Bedienung kombinieren die Spalte Art in der Tabelle ST_AlarmAktuell / ST_AlarmHistorisch
						string keyArtHinweis = string.Format("T.{0}_{1}", "Art_Hinweis", alarmIndex.ToString().PadLeft(3, '0'));
						string keyArtBedienung = string.Format("T.{0}_{1}", "Art_Bedienung", alarmIndex.ToString().PadLeft(3, '0'));
						if(telegram.Data.ContainsKey(keyArtHinweis) && telegram.Data.ContainsKey(keyArtBedienung))
						{
							//Vorher abfragen, damit es kompatibel bleibt mi GAT in Limburg und Dietermann in Viersen
							Boolean artHinweis = Convert.ToBoolean(telegram.Data[keyArtHinweis]);
							Boolean artBedienung = Convert.ToBoolean(telegram.Data[keyArtBedienung]);
							teleArt = CreateDatenbankEintragArt(artHinweis, artBedienung);
						}
						else
						{
							//Unbekannt da nicht von der SPS übertragen, weil nicht konfiguriert in der Tabelle DC_DatenClientsTIA_Items
							teleArt = "U";
						}

						string keyNichtSpeichern = string.Format("T.{0}_{1}", "nicht_speichern", alarmIndex.ToString().PadLeft(3, '0'));
						if(telegram.Data.ContainsKey(keyNichtSpeichern))
						{
							//Vorher abfragen, damit es kompatibel bleibt mi GAT in Limburg und Dietermann in Viersen
							teleNichtSpeichern = Convert.ToBoolean(telegram.Data[keyNichtSpeichern]);
						}

						string keyNichtAnzeigen = string.Format("T.{0}_{1}", "nicht_anzeigen", alarmIndex.ToString().PadLeft(3, '0'));
						if(telegram.Data.ContainsKey(keyNichtAnzeigen))
						{
							//Vorher abfragen, damit es kompatibel bleibt mi GAT in Limburg und Dietermann in Viersen
							teleNichtAnzeigen = Convert.ToBoolean(telegram.Data[keyNichtAnzeigen]);
						}



						string keySchritt = string.Format("T.{0}_{1}", "Schritt_Nr", alarmIndex.ToString().PadLeft(3, '0'));
						teleSchritt = Convert.ToByte(telegram.Data[keySchritt]);

						keyOperation = string.Format("T.{0}_{1}", "U_O", alarmIndex.ToString().PadLeft(3, '0'));

					}
					catch(Exception e123)
					{
						throw new Exception("Handle Telegram T POS 1 " +  e123.Message);
					}
               

               try
               {
                  //Funktioniert bei UA auf SPS
                  //Value ist CHAR und steht auf 85 ! Also Code für 'U'
                  teleOperation = Convert.ToChar(Convert.ToByte(telegram.Data[keyOperation]));                  
               }
               catch
               {
                  //Funktioniert bei TANI
                  //Value ist string und steht z.B. auf 'U'

						try
						{
							teleOperation = Convert.ToChar(telegram.Data[keyOperation]);
						}
						catch(Exception e555)
						{
							throw new Exception("Handle Telegram T POS 2 " +  e555.Message + "telegram.Data[keyOperation] = " + telegram.Data[keyOperation]);
						}
               }

					try
					{
						string keyParatyp = string.Format("T.{0}_{1}", "ParaTyp", alarmIndex.ToString().PadLeft(3, '0'));
						teleParaTyp = telegram.Data[keyParatyp].ToString();

						string keyDBNr = string.Format("T.{0}_{1}", "DB-Nr", alarmIndex.ToString().PadLeft(3, '0'));
						teleDBNr = telegram.Data[keyDBNr].ToString();

						string keyParam = string.Format("T.{0}_{1}", "Param", alarmIndex.ToString().PadLeft(3, '0'));
						teleParam = telegram.Data[keyParam].ToString();
               

						string keyVKTyp = string.Format("T.{0}_{1}", "VKTyp", alarmIndex.ToString().PadLeft(3, '0'));
					

						try
						{
							//Siehe Erklärung  teleOperation
							vkTyp = Convert.ToChar(Convert.ToByte(telegram.Data[keyVKTyp]));
						}
						catch
						{
							try
							{
								//Siehe Erklärung  teleOperation
								vkTyp = Convert.ToChar(telegram.Data[keyVKTyp]);
							}
							catch(Exception e777)
							{
								throw new Exception("Handle Telegram T POS 3 " +  e777.Message + "telegram.Data[keyVKTyp] = " + telegram.Data[keyVKTyp]);
							}
							
						}

						if (vkTyp == ' ')
						{
							teleBedingung = 0; //AlarmStatusID in der Tabelle ST_AlarmAktual
						}
						else  //vkTyp == 'N'
						{
							teleBedingung = 1;
						}

						teleOperand = CreateOperand(teleParaTyp, teleDBNr, teleParam);
					}
					catch(Exception e234)
					{
						throw e234;
					}


               stopUhr.Start();
                  
               DateTime localTime = DateTime.Now;

					try
					{

						if (!AktuellenAlarmEintragen(StoerBerNurAnz(teleBereichStream), (long)teleAnlagenID, teleArt, teleNichtSpeichern, teleNichtAnzeigen, (int)teleBereichStream, teleProgramNummer, (short)teleSchritt, string.Format("{0}", teleOperation), teleOperand, (int)teleBedingung, localTime))
						{
							stopUhr.Stop();
							LogManager.GetSingleton().ZLog("C0045", ELF.ERROR, "Eintragen eines Alarms fehlgeschlagen ! Weitere Auswertung abgebrochen ! ProgramNummer = {0}, Schritt = {1} , Operation = {2}, Operand = {3}, Bedingung = {4} DatenClient = {5} Telegram T = {6} Dauer = {7}", teleProgramNummer, teleSchritt, teleOperation, teleOperand, teleBedingung, datenClientID, "", stopUhr.PeriodMilliSeconds, localTime.ToString("dd.MM.yyyy HH:mm:ss.fff"));       
							return;
						}
					}
					catch(Exception e345)
					{
						throw e345;
					}

               stopUhr.Stop();
               LogManager.GetSingleton().ZLog("C0046", ELF.INFO, "S7-Alarm eingetragen: Dauer={5} ms;Zeit={6} ProgrammNummer={0}, Schritt={1}, Operation={2}, Operand={3}, Bedingung={4}", teleProgramNummer, teleSchritt, teleOperation, teleOperand, teleBedingung, stopUhr.PeriodMilliSeconds, DateTime.Now);
            }

				try
				{

					stopUhr.Start();

					HistorischenAlarmEintragen();

					stopUhr.Stop();

					LogManager.GetSingleton().ZLog("C0047", ELF.INFO, "TIA-Alarm(e) Historisch schreiben: Dauer = {0} ms", stopUhr.PeriodMilliSeconds);

					quit = true;

					stoerQuittung++;

					if (stoerQuittung > 9999)
					{
						stoerQuittung = 0;
					}
					
					
					quitValue = stoerQuittung;

					quitValue = ((TelegramT)telegram).IndexTelegramS;
				}
				catch(Exception e567)
				{
					throw e567;
				}
         }
         catch(Exception e)
         {
            LogManager.GetSingleton().ZLog("C0048", ELF.ERROR, "Exception in HandleTelegramT.HandleTelegramT -> {0}", e.Message);
         }
         
      }

      private string CreateDatenbankEintragArt(Boolean artHinweis, Boolean artBedienung)
      {
         if(artHinweis)
         {
            return "H";
         }
         if(artBedienung)
         {
            return "B";
         }

         //Wenn kein Hinweis und keine Bedienung dann ist es eine Störung
         return "S";
      }

      protected override Boolean AktuellenAlarmEintragen(Boolean nurAnz, long anlagenID, string teleArt, Boolean teleNichtSpeichern, Boolean teleNichtAnzeigen, int alarmBereichId, string programmNummer, short schritt, string operation, string operand, int alarmStatusId, DateTime currentDate)
      {
         return base.AktuellenAlarmEintragen(nurAnz, anlagenID, teleArt, teleNichtSpeichern, teleNichtAnzeigen, alarmBereichId, programmNummer, schritt, operation, operand, alarmStatusId, currentDate);
      }

      protected void HistorischenAlarmEintragen()
      {
         string query = "";

         try
         {
				if(programmNummerAlt)
				{					
					query = string.Format("insert into ST_AlarmHistorisch (Anlagen_ID, Operand, AlarmBereich_ID, Programm_Nr, Schritt, Operation, AlarmStatus_ID, TS_Beginn, TS_Quittiert, TS_Ende) ");
					query += string.Format("select Anlagen_ID, Operand, AlarmBereich_ID, Programm_Nr, Schritt, Operation, AlarmStatus_ID, TS_Beginn, TS_Quittiert, TS_Ende ");
					query += string.Format("from ST_AlarmAktuell where ActualFlag = 0 and Anlagen_ID IN ({0})", NebenAnlagenForSql);
				}
				else
				{
					query = string.Format("insert into ST_AlarmHistorisch (Anlagen_ID, Operand, AlarmBereich_ID, ProgrammNummer, Schritt, Operation, AlarmStatus_ID, TS_Beginn, TS_Quittiert, TS_Ende, Art, NichtAnzeigen) ");
					query += string.Format("select Anlagen_ID, Operand, AlarmBereich_ID, ProgrammNummer, Schritt, Operation, AlarmStatus_ID, TS_Beginn, TS_Quittiert, TS_Ende, Art, NichtAnzeigen ");
					query += string.Format("from ST_AlarmAktuell where ActualFlag = 0 and Anlagen_ID IN ({0}) and NichtSpeichern = 0", NebenAnlagenForSql);
				}

            SqlRealtimeSimpleExecute exec = new SqlRealtimeSimpleExecute(database, query);
         }
         catch (Exception e)
         {
            LogManager.GetSingleton().ZLog("C0049", ELF.ERROR, "Alarming.HistorischenAlarmEintragen (Alarm historisch schreiben) -> {0} #{1}#", e.Message, query);

            return;
         }

         try
         {
            query = string.Format("delete from ST_AlarmAktuell where (ActualFlag = 0 or ActualFlag = 3) and Anlagen_ID IN ({0})", NebenAnlagenForSql);
            SqlRealtimeSimpleExecute exec = new SqlRealtimeSimpleExecute(database, query);
         }
         catch (Exception e)
         {
            LogManager.GetSingleton().ZLog("C004A", ELF.ERROR, "Alarming.HistorischenAlarmEintragen (Löschen aus ST_AlarmAktuell) -> {0}", e.Message);
            return;
         }
      }

      private bool AktuellenAlarmAustragen()
      {
         string query = "";
         string currentDate = string.Format(" CONVERT(datetime, '{0}', 121)", DateTime.Now.ToString("yyyyMMdd HH:mm:ss"));

         try
         {
            query = string.Format("update ST_AlarmAktuell set ActualFlag = 0, TS_Ende = {0} where ActualFlag = 1 and Anlagen_ID IN ({1})", currentDate, NebenAnlagenForSql);



            SqlRealtimeSimpleExecute exec = new SqlRealtimeSimpleExecute(database, query);
         }
         catch (Exception e)
         {
            LogManager.GetSingleton().ZLog("C004B", ELF.ERROR, "AktuellenAlarmAustragen1 -> {0}", e.Message);
            LogManager.GetSingleton().ZLog("C004C", ELF.ERROR, "STATEMENT = {0}", query);
            return false;
         }

         try
         {
            query = string.Format("update ST_AlarmAktuell set ActualFlag = 3, TS_Ende = {0} where ActualFlag = 2 and Anlagen_ID IN ({1})", currentDate, NebenAnlagenForSql);
            SqlRealtimeSimpleExecute exec = new SqlRealtimeSimpleExecute(database, query);
         }
         catch (Exception e)
         {
            LogManager.GetSingleton().ZLog("C004D", ELF.ERROR, "AktuellenAlarmAustragen2 -> {0}", e.Message);
            LogManager.GetSingleton().ZLog("C004E", ELF.ERROR, "STATEMENT = {0}", query);
            return false;
         }

         return true;
      }
   }
}
