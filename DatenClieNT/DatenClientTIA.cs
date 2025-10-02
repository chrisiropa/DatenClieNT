using System;

namespace DatenClieNT
{
   class DatenClientTIA : DatenClient
   {
      public DatenClientTIA() : base ("DatenClientTIA")
      {
         systemType = SystemType.TIA;
      }

      
      public override Boolean Init(long id)
      {
         if(!base.Init(id))
         {
            return false;
         }

         alarming = new AlarmingTIA();

         if (!alarming.Init(database, this, id, anlagenID, nebenAnlagen))
         {
            return false;
         }

         daten = new DatenTIA();
         
         if(!daten.Init(database, this, id, name, anlagenID, nebenAnlagen, this))
         {
            return false;
         }
         
         return true;
      }

      public override void NotifyTelegramA(TelegramA telegramA)
      {  
         daten.HandleTelegramA(telegramA);
      }

      public override void NotifyTelegramD(TelegramD telegramD)
      {
			//Datenempfang wird signalisiert
         if (!SpsCommunicationOPC.QualityGOOD(telegramD.Quality))
         {
            LogManager.GetSingleton().ZLog("C00C6", ELF.WARNING, "NotifyTelegramD: Qualität ungleich GOOD -> {0}=Quality({1})", telegramD.SubscriptionItem, telegramD.Quality);
            return;
         }
         
         //Inhalt von Telegram D merken....
         daten.HandleTelegramD(telegramD);
         
         if(daten.ReadTelegramE)
         {
            //...dann Telegram E anfordern
            timerManager.AddSingle(TimerForceTelegramE, telegramD, 1);
         }
      }
      
      

      public override void NotifyTelegramE(TelegramE telegramE)
      {
         Telegram telegramM = spsComunicationBase.NewTelegramInstance("M");

         Boolean onlyQuit = daten.HandleTelegramE(telegramE, telegramM);

			HiresStopUhr stopUhr = new HiresStopUhr();
         
         if(onlyQuit)
         {
            //Der Steuerung den Empfang quittieren
            DataAcknoledge();
         }
         else
         {
            if (telegramM != null)
            {
               try
               {
                  spsComunicationBase.SendTelegram(telegramM);

						stopUhr.Start();

                  if(!telegramM.ErrorOccured)
                  {
                     LogManager.GetSingleton().ZLog("C00C7", ELF.INFO, "Daten wurden gesendet (Telegram 'M') -> {0}", telegramM.SendVisuData);                     
                  }
                  else
                  {
                     //Vermutlich Wandlungsfehler (Diskrepanz Datentyp Tabellenspalte/SPS-Item)
                     daten.DataAcknoledge.SetSpsReturnCode(SpsReturnCodes.Datenwandlung);
                  }
               }
               catch (Exception e)
               {
                  LogManager.GetSingleton().ZLog("C00C8", ELF.ERROR, "Daten können nicht gesendet werden (Telegram 'M'={1}). -> {0}", e.Message, telegramM.SendVisuData);
               }
            }
            else
            {
               LogManager.GetSingleton().ZLog("C00C9", ELF.ERROR, "Daten können nicht gesendet werden. Telegram 'M' nicht gefunden.");
            }

            DataAcknoledge();

				stopUhr.Stop();

				LogManager.GetSingleton().ZLog("CFFFF", ELF.DEVELOPER, "Zeit zwischen Daten gesendet und Daten quittiert -> {0} ms", stopUhr.PeriodMilliSeconds);
				
         }
      }
      
      
      

      public override void NotifyTelegramS(Telegram telegram)
      {
         base.NotifyTelegramS(telegram);
         
         //Störung wird signalisiert         
         if (!alarming.AktivAlarming)
         {
            LogManager.GetSingleton().ZLog("C00CA", ELF.WARNING, "StörungsÄnderung wird ignoriert, da beim Datenclient {0} das Alarming inaktiv ist", name);
            
            return;
         }

         //Quality prüfen
         if (!SpsCommunicationOPC.QualityGOOD(telegram.Quality))
         {
            LogManager.GetSingleton().ZLog("C00CB", ELF.WARNING, "NotifyTelegramS: Qualität ungleich GOOD -> {0}=Quality({1})", telegram.SubscriptionItem, telegram.Quality);
            return;
         }
         
         bool readTelegramT;

         
         alarming.HandleTelegramS(telegram, out readTelegramT);

         if(readTelegramT)
         {         
            //Telegram T beim ersten Mal sofort anfordern.
            //"T" NICHT ÄNDERN -> TAG WIRD ausgewertet !!!!!!!!!!!
            timerManager.AddSingle(TimerForceTelegramT, telegram, 1);
         }
      }
      
      public override void NotifyTelegramT(Telegram telegram)
      {
         //Störungsdaten treffen ein

         long counter = telegram.Counter;

         
         if (!SpsCommunicationOPC.QualityGOOD(telegram.Quality))
         {
            LogManager.GetSingleton().ZLog("C00CC", ELF.WARNING, "NotifyTelegramT: Qualität ungleich GOOD -> {0}=Quality({1})", telegram.SubscriptionItem, telegram.Quality);
            return;
         }
         
         Boolean quit = false;
         int quitValue = 1;
         
			try
			{
				//vorher siehe public override void ForceTelegram(string teleType, object tag)...
				alarming.HandleTelegramT(telegram, out quit, out quitValue);
			}
			catch(Exception e11)
			{
				LogManager.GetSingleton().ZLog("D0000", ELF.ERROR, "{1}: NotifyTelegramT -> {0}", e11.Message, this.Description);
				//throw e11;
			}

         if(quit)
         {
            //Telegram U -> Zähler auf ....vonDC.Störeintrag_erfolgt schreiben CG 04.09.2017
            Telegram quitTelegram = spsComunicationBase.NewTelegramInstance("U");

            if (quitTelegram != null)
            {
               
               
               quitTelegram.SendVisuData = string.Format("{0}", quitValue);
               quitTelegram.RegistrationValues["U.Störeintrag_erfolgt"] = quitValue.ToString();

               try
               {
                  spsComunicationBase.SendTelegram(quitTelegram);
               }
               catch (Exception e)
               {
                  LogManager.GetSingleton().ZLog("CD31F", ELF.ERROR, "Änderung_Störeinträge konnte nicht auf die Steuerung geschrieben werden -> {0}", e.Message);
               }
            }
            else
            {
               LogManager.GetSingleton().ZLog("CD320", ELF.ERROR, "Störung kann nicht quittiert werden. Telegram 'U' nicht gefunden.");
            }
         }
      }
      

      public override void NotifyTelegramW(Telegram telegram)
      {
         try
         {

            


            string visuData = string.Format("{0}", telegram.SubscribtionValue.ToString());

            telegram.VisuData = visuData;

            try
            {
               base.NotifyTelegramW(telegram);
            }
            catch(Exception e)
            {
               LogManager.GetSingleton().ZLog("CD34B", ELF.ERROR, "NotifyTelegramW -> Aufruf 'base.NotifyTelegramW(telegram)' geht schief");
               throw e;
            }

            /*
            Früher wurde der ganze Watchdog als aktives Telegramm behandelt.
            Jetzt reicht es wenn der Zähler aktiv geschaltet wird.
            Die anderen SPS-TIA-Variablen können passiv sein
            */


            //Alle 5 Sekunden das (PC)-Datum in OPCItem-WatchDog schreiben UND den Zähler nullen
            //Dadurch kommt es zustande, das immer von 0 bis 4 gezählt wird und das Datum immer stimmt

            

            Int64 watchdogValue = (Int64) Math.Abs(Convert.ToInt64(telegram.SubscribtionValue));

            if (watchdogValue > 4)
            {
               DateTime dt = DateTime.Now;

               watchdogValue = 0;

               telegram.RegistrationValues.Clear();

               telegram.RegistrationValues["W.Wochentag"] = ((Int32)(1 + dt.DayOfWeek)).ToString();
               telegram.RegistrationValues["W.Tag"] = dt.Day.ToString();
               telegram.RegistrationValues["W.Monat"] = dt.Month.ToString();
               telegram.RegistrationValues["W.Jahr"] = dt.Year.ToString();

               telegram.RegistrationValues["W.Stunde"] = dt.Hour.ToString();
               telegram.RegistrationValues["W.Minute"] = dt.Minute.ToString();
               telegram.RegistrationValues["W.Sekunde"] = dt.Second.ToString();

               telegram.RegistrationValues["W.Watchdog"] = watchdogValue.ToString();

               telegram.SetTxData(watchdogValue);
               telegram.SendVisuData = visuData;

					//Neu seit 14.01.2021 Watchdog nicht mehr jede Sekunde in die SYS_Modulüberwachung schreiben
					UpdateSysModulueberwachung();
               
               try
               {
                  spsComunicationBase.SendTelegram(telegram);
               }
               catch (Exception e)
               {
                  LogManager.GetSingleton().ZLog("C00D0", ELF.ERROR, "Watchdog konnte nicht auf die Steuerung geschrieben werden -> {0}", e.Message);
               }
            }
         }
         catch (Exception e)
         {
            LogManager.GetSingleton().ZLog("C00D1", ELF.ERROR, "DatenClientS7.NotifyTelegramW -> {0}", e.Message);
         }
      }
   }
}
