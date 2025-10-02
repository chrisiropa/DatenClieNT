using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Drawing;
using System.Data.SqlClient;

namespace DatenClieNT
{
   public enum SystemType
   {
      Unknown = 0,
      TIA = 9
   }
   

   abstract class DatenClient : WorkerThreadWrapper
   {  
      private Int64 telegramID = 1;
      private Boolean running = false;
      
      private Boolean panicRestarting = false;
		public static string ImmediateRestartTag = "IMMEDIATE_RESTART";
      
      protected Alarming alarming;
      protected Daten daten;
      protected SpsComunicationBase spsComunicationBase;
      
      protected string name;

      private Boolean suspendPiping = false;

      protected volatile int currentStoerCounter = -1;
      
      private volatile bool internRestart = true;            
      private Queue evtQueue;
      protected TimerManager timerManager = null;
      private long timestampDC_Datenclients = long.MaxValue;
      private long timestampDC_Aufträge = long.MaxValue;
      private long timestampDC_AuftragsDetails = long.MaxValue;
      private long countDC_Aufträge = 0;
      private long countDC_AuftragsDetails = 0;
      private Boolean telegramsOK = false;
      
                  
      private string opcServerName;
      protected SystemType systemType;
      protected SpsComunicationBase.ComType comType = SpsComunicationBase.ComType.OpcUA;
      protected string anlagenSymbol;

		public static Dictionary<int, string> Threads = new Dictionary<int, string>();
      
      
      protected string Description
      {
         get
         {
            return string.Format("{0} ({1})", opcServerName, Name);
         }
      }
      
      
      private Boolean aktive;
      protected Boolean aktivDatenEmpfang;
      protected Boolean aktivDatenSenden;
		protected int startOffsetArrays = 0;
		protected Boolean standardAlarming = true;
      
      protected string database;
      private long id;
      protected long anlagenID;
      protected Dictionary<long, long> nebenAnlagen = new Dictionary<long, long>(); 
      
      private int checkTelegramCounter = 0;
      
      private Watchdog watchdog = null;
      
      
      public string Name 
      {
         get { return name; }
      }

      protected long Id
      {
         get { return id; }
      }
      
      public string Database
      {
         get { return database; }
      }

		public int StartOffsetArrays
		{
			get { return startOffsetArrays; }
		}

		public Boolean StandardAlarming
		{
			get { return standardAlarming; }
		}
      
      
      public abstract void NotifyTelegramA(TelegramA telegramA);
      public abstract void NotifyTelegramT(Telegram telegram);
      public abstract void NotifyTelegramD(TelegramD telegramD);
      public abstract void NotifyTelegramE(TelegramE telegramE);

      public virtual void NotifyTelegramS(Telegram telegram)
      {
         ResetStoerCounter();
      }
      

      public virtual void NotifyTelegramW(Telegram telegram)
      {

			spsComunicationBase.RegularStartDone();

			telegramsOK = true;
         
         if(watchdog.NotifyTelegram())
         {
            LogManager.GetSingleton().ZLog("C0302", ELF.INFO, "SUCCESS-> Startphase abgeschlossen: WD kam das erste Mal ! Gesamtdauer: {0}ms", spsComunicationBase.ConnectDelay);
         }
         else
         {
            //Erster Watchdog wurde zwar empfangen aber das ist nur der initiale Wert in der SPS.
            //Ob er sich wirklich bewegt (hochzählt) weiß man hier noch nicht !
            //Daher noch keinen Vollzug melden !
         }
      }

      public virtual Boolean Init(long id)
      {
         this.id = id;

         
         telegramsOK = false;
         ResetStoerCounter();

         while (TheDC.GetSingleton().GetDatabaseConnectionProvider(database).ForceDatabaseConnection() == null)
         {
            LogManager.GetSingleton().ZLog("C0017", ELF.WARNING, "(INIT) -> Datenbank nach Störung noch nicht wieder erreichbar... -> {0}", ConfigManager.GetSingleton().SpecificConnectionString(database));
            Thread.Sleep(ConfigManager.GetSingleton().TryDatabaseDelay);
         }

         try
         {
            SqlRealtimeSimpleQuery query = new SqlRealtimeSimpleQuery(database, string.Format("select *, anl.Symbol as Symbol from DC_DatenclientsTIA, Org_Anlagen anl where DC_DatenclientsTIA.id = {0} and anl.ID = DC_DatenclientsTIA.Anlagen_ID", id));
            
            if (query.QueryResult != null)
            {
               foreach (Dictionary<string, object> prm in query.QueryResult)
               {
                  try
                  {
                     anlagenSymbol = Tools.GetString(prm, "Symbol");                     
                     name = Tools.GetString(prm, "Bezeichnung");                     
                     anlagenID = Tools.GetLong(prm, "Anlagen_ID");
                     opcServerName = Tools.GetString(prm, "OpcServerName");
                     aktive = Tools.GetBoolean(prm, "Aktiv");
                     aktivDatenEmpfang = Tools.GetBoolean(prm, "AktivDatenEmpfang");
                     aktivDatenSenden = Tools.GetBoolean(prm, "AktivDatenSenden");

							try
							{
								startOffsetArrays = (int) Tools.GetLong(prm, "StartoffsetArrays");
							}
							catch
							{
								startOffsetArrays = 0;
								LogManager.GetSingleton().ZLog("D0003", ELF.WARNING, "DC Konfiguration StartoffsetArrays nicht in DC_Datenclients_TIA vorhanden. Standardwert = 0 wird verwendet");
							}

							try
							{
								standardAlarming = Tools.GetBoolean(prm, "StandardAlarming");
							}
							catch
							{
								standardAlarming = true;
								LogManager.GetSingleton().ZLog("D0006", ELF.ERROR, "DC Konfiguration StandardAlarming nicht in DC_Datenclients_TIA vorhanden. Standardwert = true wird zwar verwendet. Aber zur Sicherheit bitte die Spalte einfügen und auf true oder false setzen !");
							}

							
                     nebenAnlagen.Clear(); //Wichtig !
                     //AnlagenID auf jeden Fall mit übergeben
                     nebenAnlagen[anlagenID] = anlagenID;

                     
                     try
                     {
                        SqlRealtimeSimpleQuery queryNebenAnlagen = new SqlRealtimeSimpleQuery(database, string.Format("select * from Org_NebenAnlagen n, ORG_Anlagen a where n.HauptAnlage = {0} and n.NebenAnlage = a.ID", anlagenID));
                        if (query.QueryResult != null)
                        {
                           string nebenAnlagenText = "";                           
                           string trennzeichen = "";
                           
                           foreach (Dictionary<string, object> nebenAnlage in queryNebenAnlagen.QueryResult)
                           {
                              nebenAnlagen[(Convert.ToInt64(Tools.GetLong(nebenAnlage, "NebenAnlage")))] = (Convert.ToInt64(Tools.GetLong(nebenAnlage, "NebenAnlage")));

                              nebenAnlagenText += string.Format("{0}{1}", trennzeichen, Tools.GetString(nebenAnlage, "Symbol"));
                              trennzeichen = "; ";
                           }
                        }
                     }
                     catch(Exception e)
                     {
                        LogManager.GetSingleton().ZLog("C0018", ELF.ERROR, "Error in DatenClient.Init() NebenAnlagen {1}-> {0}", e.Message, name);
                        return false;
                     }
                  }
                  catch(Exception e)
                  {
                     LogManager.GetSingleton().ZLog("C0019", ELF.ERROR, "Error in DatenClient.Init() {1}-> {0}", e.Message, name);
                     return false;
                  }
               }
            }
         }
         catch(Exception e)
         {
            LogManager.GetSingleton().ZLog("C001A", ELF.ERROR, "Error in DatenClient.Init() -> {0}", e.Message);
            return false;
         } 
         
         InitSysModulueberwachung();
         
         return true;
      }
      public static Boolean IsClientAuftrag(FktNr funktionsNummer)
      {
         return (funktionsNummer == FktNr.FN11WrColUpdEvtDC ||
                 funktionsNummer == FktNr.FN12WrRowInsEvtDC ||
                 funktionsNummer == FktNr.FN13WrColInsEvtDC);
      }
      

      
      
      private Boolean StartSpsCommunication()
      {
         if (aktive)
         {
            LogManager.GetSingleton().ZLog("C001B", ELF.INFO, "Start SPS Kommunikation...");

            
            
            
            switch(comType)
            {            
               case SpsComunicationBase.ComType.OpcUA: 
                  spsComunicationBase = SpsComunicationBase.ProduceOPC(daten.SpsAuftraege, daten.ClientAuftraege, anlagenID, database, systemType, NewEvent, opcServerName, "", id, name, startOffsetArrays, standardAlarming);
                  LogManager.GetSingleton().ZLog("CD031", ELF.INFO, "------------------------------ OPC-UA-MODUS -------------------");
               break;
               default:
                  spsComunicationBase = null;
               break;
            }
            
            if(spsComunicationBase != null)
            {
               if(!spsComunicationBase.Start())
               {
                  LogManager.GetSingleton().ZLog("CD215", ELF.ERROR, "spsComunicationBase.Start() fehlgeschlagen");
                  return false;
               }
               else
               {
                  LogManager.GetSingleton().ZLog("C001C", ELF.INFO, "Gestartet SPS Kommunikation.");
               }               
            }
            else
            {
               LogManager.GetSingleton().ZLog("C001D", ELF.ERROR, "SPS Kommunikation konnte nicht gestartet werden.");
            }
            
            watchdog = new Watchdog();
            watchdog.Start();
         }
         else
         {
            LogManager.GetSingleton().ZLog("C001E", ELF.WARNING, "NICHT gestartet SPS Kommunikation. Datenclient inaktiv.");
            spsComunicationBase = null;
         }
         
         return true;
      }      
      private void StopSpsCommunication()
      {
         if (spsComunicationBase != null && spsComunicationBase.Running)
         {
            LogManager.GetSingleton().ZLog("C001F", ELF.INFO, "Stop SPS Kommunikation...");

            try
            {
               watchdog.Stop();
            }
            catch
            {
               LogManager.GetSingleton().ZLog("CD34D", ELF.ERROR, "Dieser Fehler kommt, wenn es keine Items in der Tabelle DC_DatenClientsTIA_Items zu der richtigen DatenClient ID gibt (Tabelle DC_DatenClientsTIA)");
            }
            
            spsComunicationBase.Stop();

            LogManager.GetSingleton().ZLog("C0020", ELF.INFO, "Gestoppt SPS Kommunikation.");
         }
      }
      private void StartTimerManager()
      {
         LogManager.GetSingleton().ZLog("C0074", ELF.INFO, "Start TimerManager...");
         
         timerManager = TimerManager.NewInstance(NewEvent, "TIMER");
         timerManager.Start();

         InitTimerEvents();

         LogManager.GetSingleton().ZLog("C0075", ELF.INFO, "Gestartet TimerManager.");
      }

      private void StopTimerManager()
      {
         LogManager.GetSingleton().ZLog("C0076", ELF.INFO, "Stop TimerManager...");
         
         timerManager.Stop();

         LogManager.GetSingleton().ZLog("C0077", ELF.INFO, "Gestoppt TimerManager.");
      }

      
      public override bool Stop()
      {
         if(!running)
         {
            return true;
         }

         LogManager.GetSingleton().ZLog("C0078", ELF.INFO, "Stop Datenclient -> {0}", opcServerName);
         
         NewEvent(Evt.NewTerminateEvt());
         
         workerThread.Join(6000);
         
         if(workerThread.IsAlive)
         {
            LogManager.GetSingleton().ZLog("C0079", ELF.INFO, "Gestoppt Datenclient -> {0}", opcServerName);

            Thread.Sleep(2000);

            Environment.Exit(0);
         }
         
         return true;
      }

      public override bool Start()
      {
         LogManager.GetSingleton().ZLog("C008A", ELF.INFO, "Start Datenclient -> {0}", Description);
         
         checkTelegramCounter = 0;
         evtQueue = Queue.Synchronized(new Queue(50000));
         ResetStoerCounter();
         
         base.RunWorkerThread(EventThreadFunction, string.Format("{0}", Description));


         LogManager.GetSingleton().ZLog("C008B", ELF.INFO, "Gestartet Datenclient -> {0}", Description);

         return true;
      }

      private void StopIntern()
      {
         terminate = true;
         internRestart = true;     
      }

      private void StartIntern()
      {
         while (TheDC.GetSingleton().GetDatabaseConnectionProvider(database).ForceDatabaseConnection() == null)
         {
            LogManager.GetSingleton().ZLog("C008C", ELF.WARNING, "(StartIntern) -> Datenbank nach Störung noch nicht wieder erreichbar... -> {0}", ConfigManager.GetSingleton().MainConnectionString);
            Thread.Sleep(ConfigManager.GetSingleton().TryDatabaseDelay);

            evtQueue.Clear();
         }

         LogManager.GetSingleton().ZLog("C008D", ELF.INFO, "Intern Start Datenclient -> {0}", opcServerName);

         checkTelegramCounter = 0;
         evtQueue = Queue.Synchronized(new Queue(50000));
         ResetStoerCounter();

         LogManager.GetSingleton().ZLog("C008E", ELF.INFO, "Intern Gestartet Datenclient -> {0}", opcServerName);
      }
      

      private void NotifyTerminate()
      {
         terminate = true;
         internRestart = false;  
      }
      

      protected DatenClient(string name)
         : base(name)
      {         
      }

      public static DatenClient NewDatenClient(SystemType systemType)
      {
         if (systemType == SystemType.TIA)
         {
            return new DatenClientTIA();
         }
         
         return null;
      }
      
      private void InitTimerEvents()
      {
         if(aktive)
         {
            timerManager.AddSingle(CheckTelegrams, "CheckTelegrams", 300);
            timerManager.AddSingle(TimerCheckSpsCommunicationConnection, "TimerCheckSpsCommunicationConnection", 3000);
         }

         timerManager.AddSingle(TimerCheckDatabaseAndConfig, "TimerCheckDB", 10000);
         
         if(aktive)
         {
            LogManager.GetSingleton().ZLog("CD261", ELF.INFO, "Watchdog initial anmelden");
            timerManager.AddSingle(TimerCheckWatchdog, string.Format("{0} WD", name), 1750);
            
        
            foreach (Auftrag clientAuftrag in daten.ClientAuftraege.Values)
            {
               timerManager.AddSingle(TimerClientAuftrag, clientAuftrag, clientAuftrag.UpdateIntervall);
            }
         }
         
      }
      
      private void EventThreadFunction()
      {
         running = true;

			lock(DatenClient.Threads)
			{
				DatenClient.Threads[Thread.CurrentThread.ManagedThreadId] = string.Format("{0}|EVT_THREAD", Thread.CurrentThread.Name);
			}
         
         while(internRestart)
         {
            internRestart = true;
            int remainingEvents = -1;
                     
            StartTimerManager();       
            
              
            if(!StartSpsCommunication())
            {
               StopTimerManager();
               StopSpsCommunication();

               LogManager.GetSingleton().ZLog("CD217", ELF.WARNING, "Fehler beim Restart. Nochmal versuchen in 3 Sekunden....");
               Thread.Sleep(ConfigManager.GetSingleton().TryDatabaseDelay);
               continue;
            }
                    
                        
            Evt evt;
            
            terminate = false;

            while (!terminate)
            {
               while (((remainingEvents = evtQueue.Count) != 0) && (!terminate))
               {
                  try
                  {
                  
                     evt = (Evt)evtQueue.Dequeue();

							
                     if(terminate)
                     {
                        continue;
                     }
                     
                     try
                     {
                        NotifyEvent(evt, remainingEvents - 1);
                     }
                     catch(Exception e1)
                     {
                        throw new Exception(string.Format("Exception in EventThreadFunction POS1 -> {0} -> StackTrace = {1}",e1.Message, e1.StackTrace));
                     }
                  }
                  catch (Exception e)
                  {
                     //Zur Sicherheit Störcounter resetten, damit er nicht in einen Zustand gerät
                     //der dazu führt, daß keine Telegram S mehr in die Queue aufgenommen werden.
                     ResetStoerCounter();
                     LogManager.GetSingleton().ZLog("C008F", ELF.ERROR, "Exception in EventThreadFunction POS2 -> {0} StackTrace = {1}", e.Message, e.StackTrace);
                  }
               }

               if ((!terminate) && (evtQueue.Count == 0))
               {
                  lock (evtQueue)
                  {
                     if (evtQueue.Count == 0)
                     {
                        Monitor.Wait(evtQueue);
                     }
                  }
               }
            }
            while ((remainingEvents = evtQueue.Count) != 0)
            {
               try
               {
                  evt = (Evt)evtQueue.Dequeue();

                  if (terminate)
                  {
                     continue;
                  }

                  NotifyEvent(evt, remainingEvents);
               }
               catch (Exception e)
               {
                  LogManager.GetSingleton().ZLog("C0090", ELF.ERROR, "Exception in EventThreadFunction POS3 -> {0}", e.Message);
               }
            }

            StopTimerManager();
            StopSpsCommunication();
         }
         
         running = false;
      }
      
      public void NewEvent(Evt evt)
      {
         evt.PipeStamp = DateTime.UtcNow;
         
         
         
         
         if(evt.EvtType == Evt.Type.Telegram)
         {
            EvtTelegram telegramEvt = (EvtTelegram)evt;
         }
         
         
         if(evt.EvtType == Evt.Type.SuspendPiping)
         {
            LogManager.GetSingleton().ZLog("CD061", ELF.WARNING, "EventQueue nach Lesefehler inaktiv !");
            suspendPiping = true;
            return;
         }
         else if (evt.EvtType == Evt.Type.ResumePiping)
         {
            LogManager.GetSingleton().ZLog("CD062", ELF.WARNING, "EventQueue wieder verfügbar !");
            suspendPiping = false;
            return;
         }

         if (suspendPiping)
         {
            return;
         }
         
         if(evt.EvtType == Evt.Type.Telegram)
         {
            EvtTelegram telegramEvt = (EvtTelegram)evt;

            if (systemType == SystemType.TIA)
            {
               if (telegramEvt.Telegram.TeleType == "S")
               {
                  if(currentStoerCounter > (-1))
                  {
                     LogManager.GetSingleton().ZLog("CD036", ELF.INFO, "Eintragen von Telegram S in die Queue verhindert, da schon vorhanden !");
                     return;
                  }
                  else
                  {
                     try
                     {
                        Interlocked.Exchange(ref currentStoerCounter, Convert.ToInt32(telegramEvt.Telegram.SubscribtionValue));
                     }
                     catch
                     {
                        ResetStoerCounter();
                        LogManager.GetSingleton().ZLog("CD037", ELF.ERROR, "Wandeln des Störcounters fehlgeschlagen !");
                     }
                  }                
               }     
            }       
         }
         
         int queueSize = evtQueue.Count;
         
         try
         {
				evtQueue.Enqueue(evt);  
         }
         catch(Exception e)
         {
            LogManager.GetSingleton().ZLog("CD033", ELF.ERROR, "PANIC ! NewEvent->Beim Schreiben in die Queue -> {0} -> Stacktrace -> {1}", e.Message, e.StackTrace);
            LogManager.GetSingleton().ZLog("CD034", ELF.ERROR, "PANIC ! Anzahl Einträge in Queue vor Absturz -> {0}", queueSize);
            
            throw e;
         }

         lock (evtQueue)
         {
            Monitor.PulseAll(evtQueue);
         }

         //Thread.Sleep(200);
      }

      private void NotifyEvent(Evt evt, int remainingEvents)
      {
         Boolean logged = false;
      
         if(evt.PipeDelay > 5000)
         {
            logged = true;
            LogManager.GetSingleton().ZLog("C0091", ELF.WARNING, "NotifyEvent -> EVENT war mehr als 5000 ms in der EventQueue. EVT = {0} PipeDelay = {1} RemainingEvents = {2}", evt.ToString(), evt.PipeDelay, remainingEvents);
         }
         if(remainingEvents > 50)
         {
            logged = true;
            LogManager.GetSingleton().ZLog("C0092", ELF.WARNING, "NotifyEvent -> Mehr als 50 Events sind noch in der EventQueue. EVT = {0} PipeDelay = {1} RemainingEvents = {2}", evt.ToString(), evt.PipeDelay, remainingEvents);
         }
         
         if(!logged)
         {
            if(evt.EvtType == Evt.Type.Timer)
            {
               EvtTimer evtTimer = (EvtTimer) evt;
               
               if(evtTimer.Delay > 100)
               {
                  //Bis auf Weiteres erstmal keine Timer-Events mitloggen...
                  //LogManager.GetSingleton().ZLog("C0093", ELF.DEVELOPER, "EVT={0} Delay={1} Remaining={2}", evt.ToString(), evt.PipeDelay, remainingEvents);
               }
            }
            else
            {
               LogManager.GetSingleton().ZLog("CD026", ELF.INFO, "EVT={0} Delay={1} Remaining={2}", evt.ToString(), evt.PipeDelay, remainingEvents);
            }
         }         

			try
			{
				if (evt.EvtType == Evt.Type.Timer)
				{					
					EvtTimer timerEvt = (EvtTimer)evt;

					timerEvt.TimerEvent(timerEvt.Tag);
				}
				else if (evt.EvtType == Evt.Type.Telegram)
				{
					EvtTelegram telegramEvt = (EvtTelegram)evt;
					NotifyTelegram(telegramEvt.Telegram);
				}
				else if (evt.EvtType == Evt.Type.Terminate)
				{
					NotifyTerminate();
				}
			}
			catch(Exception eee)
			{
				throw eee;
			}
      }
      
      private void ResetStoerCounter()
      {
         Interlocked.Exchange(ref currentStoerCounter, -1);
      }
      
      private void NotifyTelegram(Telegram telegram)
      {
			telegramID++;

			         
         try
         {
            if(telegram.TeleType.Length < 1)
            {
               LogManager.GetSingleton().ZLog("C0094", ELF.ERROR, "DatenClient.NotifyTelegram: Telegram ohne teleType empfangen -> {0} -> Telegram wir ignoriert", telegram.SubscriptionItem);
               return;
            }            
            telegram.VisuData = "---";

            LogManager.GetSingleton().ZLog("CFFF0", ELF.TELE, "RX {4} ROH {0} ->  [{1}],[{2}],[{3}]", telegram.TeleType, telegram.VisuData, telegram.Quality, telegram.TimeStamp.ToString("dd.MM.yy HH:mm:ss.fff"), telegramID);

            switch(telegram.TeleType[0])
            {
               case 'W': NotifyTelegramW(telegram); break;
               
               case 'S': NotifyTelegramS(telegram); break;
               case 'T': NotifyTelegramT(telegram); break;

               case 'D': NotifyTelegramD((TelegramD)telegram); break;
               case 'E': NotifyTelegramE((TelegramE)telegram); break;
               
               case 'A': NotifyTelegramA((TelegramA)telegram); break;

               default: LogManager.GetSingleton().ZLog("C0095", ELF.WARNING, "Unbekanntes Telegram empfangen -> {0}", telegram.TeleType); break;
            }

            LogManager.GetSingleton().ZLog("CFFF1", ELF.TELE, "RX {4} Ausgew. {0} ->  [{1}],[{2}],[{3}]", telegram.TeleType, telegram.VisuData, telegram.Quality, telegram.TimeStamp.ToString("dd.MM.yy HH:mm:ss.fff"), telegramID);

            
         }
         catch(Exception e)
         {
            LogManager.GetSingleton().ZLog("C0097", ELF.ERROR, "DatenClient.NotifyTelegram {0}-> {1}", telegram.TeleType, e.Message);
         }
      }

      private void TimerCheckWatchdog(object tag)
      {
			if(System.Diagnostics.Debugger.IsAttached)
			{
				LogManager.GetSingleton().ZLog("D0008", ELF.INFO, "__________________CHECK WD");
			}

			if (!watchdog.TimedOut)
         {
            timerManager.AddSingle(TimerCheckWatchdog, string.Format("{0} WD", name), 500);
            return;
         }
         else
         {
				timerManager.AddSingle(TimerCheckWatchdog, string.Format("{0} WD", name), 500);
         }  

         LogManager.GetSingleton().ZLog("CD259", ELF.DEVELOPER, "Gültiger TimerCheckWatchdog gekommen {0}", tag);

			

         if (watchdog.Ok)
         {
				//Seit 14.01.2021 wird die Modulüberwachung nur noch alle 5 Sekunden geschrieben, wenn auch der WD zurück in die SPS geschrieben wird
            //UpdateSysModulueberwachung();

				if(System.Diagnostics.Debugger.IsAttached)
				{
					LogManager.GetSingleton().ZLog("D0009", ELF.INFO, "__________________WD OK");
				}
         }  
         else
         {
            if(spsComunicationBase.Critical())
            {
               LogManager.GetSingleton().ZLog("C009B", ELF.WARNING, string.Format("WD ERR {0}", tag));         
            }
            else
            {
               //Startphase.....
            }
            
				//10.05.2022 CG
				//Wenn nie ein Watchdog kommt, auch nicht während der Startphase, dann kommt er auch nie aus der Startphase raus
				//und er wird auch nie neu gestartet.
            spsComunicationBase.SpsUnavailable();
         }
      }

      private void InitSysModulueberwachung()
      {
         string query = "";

         try
         {
            //update nur dafür da um zu ermitteln ob Eintrag schon da. Nichts verändern.
            query = string.Format("update SYS_ModulÜberwachung set PollingTime = PollingTime where ID like 'dc_{0}' ", anlagenSymbol);
            query += "if @@rowcount = 0 begin ";
            query += string.Format("insert into SYS_ModulÜberwachung (ID, Name, PollingTime, Beschreibung) values ('dc_{0}', 'DC_{0}', 10, 'DatenClieNT der Anlage/SPS {0}') ", anlagenSymbol);
            query += " end";
            
            SqlRealtimeSimpleExecute exec = new SqlRealtimeSimpleExecute(database, query, false);
         }
         catch(Exception e)
         {
            LogManager.GetSingleton().ZLog("CD242", ELF.ERROR, "InitSysModulueberwachung -> {0}", e.Message);
         }
      }
      
      protected void UpdateSysModulueberwachung()
      {
         string query = "";

         try
         {
            query = string.Format("update SYS_ModulÜberwachung set TS_geaendert = getDate() where id = @id");

				
            SqlRealtimeExtendetExecute exec = new SqlRealtimeExtendetExecute(database, query);
            string wertID = string.Format("dc_{0}", anlagenSymbol);
            exec.AddParameter("@id", wertID);

				string log = string.Format("{0} (@id={1})",query, wertID);
				

            exec.ExecuteNonQuery(1);

				LogManager.GetSingleton().ZLog("D0011", ELF.INFO, "ModUeberw = {0}", log);

				if(exec.Exception != null)
				{
					LogManager.GetSingleton().ZLog("D0012", ELF.WARNING, "ModUeberw = {0}", exec.Exception.Message);
				}
			}
         catch(Exception e)
         {
            LogManager.GetSingleton().ZLog("CD244", ELF.WARNING, "UpdateSysModulueberwachung -> {0}", e.Message);          
         }
      }


      
      private void TimerClientAuftrag(object tag)
      {
         Auftrag auftrag = (Auftrag)tag;
         
         if (aktive && telegramsOK)
         {
            LogManager.GetSingleton().ZLog("C009E", ELF.INFO, "Client-Auftrag {0} wird bearbeitet", auftrag.AuftragsNummer);

            spsComunicationBase.ForceTelegram(string.Format("A{0}", auftrag.AuftragsNummer.ToString()), null);
         }
         else
         {
            //Bei aktivem DatenClieNT sind die Telegramme hier noch nicht alle lesbar
            //Daher noch keine ClientAufträge abarbeiten
         }

         timerManager.AddSingle(TimerClientAuftrag, auftrag, auftrag.UpdateIntervall);
      }

      
      private void TimerCheckDatabaseAndConfig(object tag)
      {
         Int64 ts = 0;
         Int64 tc = 0;
         
         HiresStopUhr stopUhr = new HiresStopUhr();
         
         stopUhr.Start();
         

         using (SqlConnection sqlConnection = TheDC.GetSingleton().GetDatabaseConnectionProvider(database).GetOpenDBConnection())
         {
            if (sqlConnection == null)
            {
               if(!panicRestarting)
               {
                  panicRestarting = true;
                  LogManager.GetSingleton().ZLog("C00A0", ELF.WARNING, "PANIC Restart-Event auslösen.....");
                  timerManager.AddSingle(TimerRestart, DatenClient.ImmediateRestartTag, 0);
                  return;
               }
               else
               {
                  LogManager.GetSingleton().ZLog("CD012", ELF.INFO, "PANIC Restart-Event wurde schon ausgelöst");
               }
               
               return;
            }
         }

         //DC_DatenclientsTIA auf Veränderung überprüfen...         
         try
         {
            SqlRealtimeSimpleQuery tsQuery = new SqlRealtimeSimpleQuery(database, "select Convert(bigint, max(TS)) as TS from DC_DatenclientsTIA where ID = {0}", id);

            if (tsQuery.QueryResult != null)
            {
               Dictionary<string, object> obj = tsQuery.QueryResult[0];

               if (obj["TS"] != System.DBNull.Value)
               {
                  ts = Convert.ToInt64(obj["TS"]);
               }
            }
         }
         catch(Exception e)
         {
            LogManager.GetSingleton().ZLog("C00A1", ELF.ERROR, "DatenClient.TimerCheckDatabaseAndConfig (DC_Datenclients) konnte nicht ausgeführt werden. Möglicherweise ist die Datenbank nicht mehr erreichbar. Es wird mit der bestehenden OPC-Konfiguration weitergemacht. -> {0}", e.Message);
            timerManager.AddSingle(TimerCheckDatabaseAndConfig, "TimerCheckDB", 10000);
            return;
         }

         if (ts > timestampDC_Datenclients)
         {
            if (timestampDC_Datenclients != 0)
            {
					timestampDC_Datenclients = ts;

               timerManager.AddSingle(TimerRestart, "TimerRestart (DC_Datenclients)", 0);
               return;
            }
         }

         timestampDC_Datenclients = ts;



         
         tc = 0;
         ts = 0;  
         try
         {
            SqlRealtimeSimpleQuery tsQuery = new SqlRealtimeSimpleQuery(database, "select count(*) as TC, Convert(bigint, max(TS)) as TS from DC_AufträgeTIA where Datenclient_ID = {0}", id);

            if (tsQuery.QueryResult != null)
            {
               Dictionary<string, object> obj = tsQuery.QueryResult[0];

               if (obj["TS"] != System.DBNull.Value)
               {
                  ts = Convert.ToInt64(obj["TS"]);
               }
               if (obj["TC"] != System.DBNull.Value)
               {
                  tc = Convert.ToInt64(obj["TC"]);
               }
            }
         }
         catch (Exception e)
         {
            LogManager.GetSingleton().ZLog("C00A2", ELF.ERROR, "DatenClient.TimerCheckDatabaseAndConfig (DC_AufträgeTIA) konnte nicht ausgeführt werden. Möglicherweise ist die Datenbank nicht mehr erreichbar. Es wird mit der bestehenden OPC-Konfiguration weitergemacht. -> {0}", e.Message);
            timerManager.AddSingle(TimerCheckDatabaseAndConfig, "TimerCheckDB", 10000);
            return;
         }

         if (ts > timestampDC_Aufträge)
         {
				timestampDC_Aufträge = ts;
            timerManager.AddSingle(TimerRestart, "TimerRestart (Auftrag geändert)", 0);  
				return;
         }
         if (tc != this.countDC_Aufträge)
         {
            if (countDC_Aufträge != 0)
            {
					countDC_Aufträge = tc;
               timerManager.AddSingle(TimerRestart, "TimerRestart (Auftrag gelöscht/hinzugefügt)", 0);
               return;
            }

            countDC_Aufträge = tc;
         }

         timestampDC_Aufträge = ts;
         
         
         ts = 0;
         tc = 0;
         try
         {
            SqlRealtimeSimpleQuery tsQuery = new SqlRealtimeSimpleQuery(database, "select count(*) as TC, Convert(bigint, max(ad.TS)) as TS from DC_AuftragsDetailsTIA ad, DC_AufträgeTIA a where Datenclient_ID = {0} and a.ID = ad.Auftrags_ID", id);

            if (tsQuery.QueryResult != null)
            {
               Dictionary<string, object> obj = tsQuery.QueryResult[0];

               if (obj["TS"] != System.DBNull.Value)
               {
                  ts = Convert.ToInt64(obj["TS"]);
               }
               if (obj["TC"] != System.DBNull.Value)
               {
                  tc = Convert.ToInt64(obj["TC"]);
               }

            }
         }
         catch (Exception e)
         {
            LogManager.GetSingleton().ZLog("C00A3", ELF.ERROR, "DatenClient.TimerCheckDatabaseAndConfig (AuftragsDetails) konnte nicht ausgeführt werden. Möglicherweise ist die Datenbank nicht mehr erreichbar. Es wird mit der bestehenden OPC-Konfiguration weitergemacht. -> {0}", e.Message);
            timerManager.AddSingle(TimerCheckDatabaseAndConfig, "TimerCheckDB", 10000);
            return;
         }

         if (ts > timestampDC_AuftragsDetails)
         {
            if (timestampDC_AuftragsDetails != 0)
            {
					timestampDC_AuftragsDetails = ts;
               timerManager.AddSingle(TimerRestart, "TimerRestart (Auftragsdetail geändert)", 0);
               return;
            }
         }

         timestampDC_AuftragsDetails = ts;
         
         if (tc != countDC_AuftragsDetails)
         {
            if (countDC_AuftragsDetails != 0)
            {
					countDC_AuftragsDetails = tc;
               timerManager.AddSingle(TimerRestart, "TimerRestart (Auftragsdetail gelöscht/hinzugefügt)", 0);
               return;
            }

            countDC_AuftragsDetails = tc;
         }



         stopUhr.Stop();

         if (stopUhr.PeriodMilliSeconds > TheDC.DatabaseCriticalDelay)
         {
            LogManager.GetSingleton().ZLog("CD103", ELF.WARNING, "TimerCheckDatabaseAndConfig dauerte länger als {0}ms -> Statement=Alle Selects aus der Funktion TimerCheckDatabaseAndConfig | DauerExecute={1}", TheDC.DatabaseCriticalDelay, stopUhr.PeriodMilliSeconds);
         }

         LogManager.GetSingleton().ZLog("CD202", ELF.DEVELOPER, "DB-DELAY {0}ms {1}", stopUhr.PeriodMilliSeconds, "Alle Selects aus der Funktion TimerCheckDatabaseAndConfig");

         timerManager.AddSingle(TimerCheckDatabaseAndConfig, "TimerCheckDB", 10000);
      }

      protected void DataAcknoledge()
      {

			//LogManager.GetSingleton().ZLog("XXXXX", ELF.ERROR, "Verzögerung eingebaut WIEDER REIN");
			//Thread.Sleep(3000);
			//LogManager.GetSingleton().ZLog("XXXXX", ELF.ERROR, "Verzögerung eingebaut WIEDER REIN");

         if (daten.DiffIDsDandE)
         {
            //Solange er in der Phase unterschiedlicher ID's ist darf er nicht quittieren.....
            LogManager.GetSingleton().ZLog("C00A4", ELF.INFO, "Phase unterschiedlicher ID's: Quittierung unterdrückt !");
            return;
         }

         HiresStopUhr stopUhr = new HiresStopUhr();
         stopUhr.Start();

         //Der Steuerung den Empfang quittieren
         Telegram quitTelegram = spsComunicationBase.NewTelegramInstance("F");

         if (quitTelegram != null)
         {
            
            quitTelegram.SendVisuData = string.Format("{0}:{1}", daten.DataAcknoledge.GetTelegramIndex(), daten.DataAcknoledge.GetSpsReturnCode());

            quitTelegram.RegistrationValues["F.DatenQuitt_Fehler"] = (Convert.ToUInt16(daten.DataAcknoledge.GetSpsReturnCode())).ToString();
            quitTelegram.RegistrationValues["F.DatenQuittierung"] = (daten.DataAcknoledge.GetTelegramIndex()).ToString();


            try
            {
					//LogManager.GetSingleton().ZLog("XXXXX", ELF.ERROR, "Quittierung ausgebaut WIEDER REIN");
               spsComunicationBase.SendTelegram(quitTelegram);
					//LogManager.GetSingleton().ZLog("XXXXX", ELF.ERROR, "Quittierung ausgebaut WIEDER REIN");


               LogManager.GetSingleton().ZLog("C00A5", ELF.INFO, "Daten quittiert (Telegram 'F') -> {0}", quitTelegram.SendVisuData);
            }
            catch (Exception e)
            {
               LogManager.GetSingleton().ZLog("C00A6", ELF.ERROR, "Daten können nicht quittiert werden (Telegram 'F'={1}). -> {0}", e.Message, quitTelegram.SendVisuData);
            }
         }
         else
         {
            LogManager.GetSingleton().ZLog("C00A7", ELF.ERROR, "Daten können nicht quittiert werden. Telegram 'F' nicht gefunden.");
         }

         stopUhr.Stop();

         LogManager.GetSingleton().ZLog("C00A8", ELF.INFO, "Quittieren in {0}ms", stopUhr.PeriodMilliSeconds);
      }

           
      private void CheckTelegrams(object tag)
      {
         //Dieser Handler zieht sich solange wieder neu auf bis alle angemeldeten Telegramme
         //einmal erfolgreich gelesen wurden. Zumindest beim INAT-OPC-Server kann man nicht direkt
         //nach dem Anmelden eines Items mit einem erfolgreichen Lesen desselben rechnen.
         //Um sicher zu gehen, das alles sauber funktioniert, wird erst mit der 
         //Telegramverarbeitung begonnen, wenn alle angemeldeten Items auch einmal erfolgreich 
         //gelesen wurden. Auch das Watchdog-Telegram wird solange unterdrückt!
         
         checkTelegramCounter++;
         
         int nextCheckTelegrams = 500;
         
         if(checkTelegramCounter > 3)
         {
            nextCheckTelegrams = 2500;
         }

         
         if (!spsComunicationBase.AllTelegramsAvailable)
         {
            LogManager.GetSingleton().ZLog("C00A9", ELF.INFO, "---------------- Noch nicht alle Telegramme verfügbar ! Nächster check in {0} ms", nextCheckTelegrams);

            //Timer solange wieder neu aufziehen, bis alle Telegramme verfügbar sind
            timerManager.AddSingle(CheckTelegrams, "CheckTelegrams", nextCheckTelegrams);

            return;
         }

         if(!spsComunicationBase.ConnectComplete())
			{
				LogManager.GetSingleton().ZLog("CFFF7", ELF.ERROR, "---------------- ConnectComplete (ApplyChanges) fehlgeschlagen ! Nächster Versuch in {0} ms", nextCheckTelegrams);

            //Timer solange wieder neu aufziehen, bis auch ApplyChanges funktioniert hat
				//Vorher können die Telegramme auch gültig sein, obwohl die SPS nicht da ist
				//Weil der TANI aber da ist.
            timerManager.AddSingle(CheckTelegrams, "CheckTelegrams", nextCheckTelegrams);

            return;
			}
			
			LogManager.GetSingleton().ZLog("CFFF9", ELF.INFO, "---------------- ConnectComplete (ApplyChanges) success");			
				
			
			if(!spsComunicationBase.MonitoredItemsCreated((checkTelegramCounter % 2) == 0))
			{
				//Auch nachdem ApplyChanges geklappt hat sind definitiv nicht (immer) alle aktiven Items Created. 
				//Meistens ist nur der Watchdog created und das liegt am Cache vom Tani (Mehrbrodt 19.03.2020)
				//Man kann aber das Created-Flag der monitored-Items gelegentlich abfragen.
				//Dieses wird über die OpcUa.dll oder wie auch immer auf true gesetzt, sobald die SPS wieder da ist.
				//Man muss also nichts erneut anmelden oder nochmal ApplyChanges aufrufen.

				//Nur jedes 10. Mal mitloggen
				if((checkTelegramCounter % 2) == 0)
				{
					LogManager.GetSingleton().ZLog("CFFFC", ELF.ERROR, "---------------- Noch nicht alle aktiven Telegramme verfügbar/created ! Nächster Versuch in {0} ms", 2000);
				}

            timerManager.AddSingle(CheckTelegrams, "CheckTelegrams", 2000);

				return;
			}
			


      
         LogManager.GetSingleton().ZLog("C00B0", ELF.INFO, "-------------------------------------------------------------");
         LogManager.GetSingleton().ZLog("C00B1", ELF.INFO, "-------------------------------------------------------------");
         LogManager.GetSingleton().ZLog("C00B2", ELF.INFO, "------- ALLE (KONFIGURIERTEN)TELEGRAMME VERFÜGBAR -----------");
         LogManager.GetSingleton().ZLog("C00B3", ELF.INFO, "--------------- Telegramverarbeitung beginnt ----------------");
         LogManager.GetSingleton().ZLog("C00B4", ELF.INFO, "-------------------------------------------------------------");
         LogManager.GetSingleton().ZLog("C00B5", ELF.INFO, "-------------------------------------------------------------");
            
         telegramsOK = true;
      }

      private void TimerRestart(object tag)
      {
         LogManager.GetSingleton().ZLog("C00B6", ELF.INFO, "Datenclient {0}: RESTART nach -> {1}", name, (string)tag);
         
         //Hier nur internen Restart machen.
                  
         StopIntern();
         
			//CG: 26012021
			//Bedingungslos neu starten inkl. Konfiguration neu einlesen. Es kam sonst häufiger vor, dass nachdem die Datenbank weg war
			//die Aufträge alle weg waren.
         //if(!panicRestarting)
         {
            //Wenn der Restart durch eine Datenbank-Änderung ausgelöst wurde 
            //dann die neue Konfiguration komplett neu einlesen.....
            Init(this.id);
         }
         
         StartIntern();
         panicRestarting = false;

         //xxxxxxxxxxxxxxxxx
      }

      
      
      protected void TimerForceTelegramT(object telegramS)
      {
         spsComunicationBase.ForceTelegram("T", telegramS);
         //Landet in.....
      }

      protected void TimerForceTelegramE(object telegramD)
      {
         spsComunicationBase.ForceTelegram("E", telegramD);
      }

      private void TimerCheckSpsCommunicationConnection(object tag)
      {
         if(spsComunicationBase == null)
         {
            LogManager.GetSingleton().ZLog("C00B7", ELF.ERROR, "Datenclient.TimerCheckSpsCommunicationConnection -> opcServer == null");
            timerManager.AddSingle(TimerCheckSpsCommunicationConnection, "TimerCheckSpsCommunicationConnection", 3000);
            return;
         }
      
         if (!spsComunicationBase.Running)
         {
            LogManager.GetSingleton().ZLog("C00B9", ELF.INFO, "Datenclient ({0}) hat keine Verbindung mehr zur SPS -> RESTART", name);

            timerManager.AddSingle(TimerRestart, DatenClient.ImmediateRestartTag, 0);
         }
         else
         {
            timerManager.AddSingle(TimerCheckSpsCommunicationConnection, "TimerCheckSpsCommunicationConnection", 3000);
         }
      }

      public void PreInit(string database)
      {
         this.database = database;
      }
   }
}
