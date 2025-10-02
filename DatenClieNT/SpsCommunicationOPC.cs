using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Opc.Ua;
using Opc.Ua.Client;

namespace DatenClieNT
{
   public class SpsCommunicationOPC : SpsComunicationBase
   {
      private DateTime lastRegularStartPhaseDone = DateTime.UtcNow;
      private Boolean regularStartPhase = true;
      
      private string opcServerName;
      private string opcRechnerName;      
      private string dcName = "";
      private long countCallbacks = 0;
      private Dictionary<string, Auftrag> spsAuftraege = null;
      private Dictionary<string, Auftrag> clientAuftraege = null;
		private Dictionary<string, MonitoredItem> monitoredItems = null;

      private Session session = null;
      private UAClientHelperAPI clientHelperAPI = null;
      private EndpointDescription theEndpoint = null;

		protected int startOffsetArrays = 0;
		protected Boolean standardAlarming = true;

		

      private Subscription subscription = null; 
      
      private SpsCommunicationOPC(Dictionary<string, Auftrag> spsAuftraege, Dictionary<string, Auftrag> clientAuftraege, long anlagenID, string database, SystemType systemType, EventDelegate eventDelegate, string opcServerName, string opcRechnerName, long datenClientID, string dcName, int startOffsetArrays, Boolean standardAlarming)
      : base(database)
      {
         this.anlagenID = anlagenID;
         this.spsAuftraege = spsAuftraege;
         this.clientAuftraege = clientAuftraege;
         this.datenClientID = datenClientID;
         this.opcServerName = opcServerName;
         this.opcRechnerName = opcRechnerName;
         this.eventDelegate = eventDelegate;
         this.systemType = systemType;
         this.dcName = dcName;
			this.startOffsetArrays = startOffsetArrays;
			this.standardAlarming = standardAlarming;

      }

      public static SpsCommunicationOPC NewInstance(Dictionary<string, Auftrag> spsAuftraege, Dictionary<string, Auftrag> clientAuftraege, long anlagenID, string database, SystemType systemType, EventDelegate eventDelegate, string opcServerName, string opcRechnerName, long datenClientID, string dcName, int startOffsetArrays, Boolean standardAlarming)
      {
         try
         {
            return new SpsCommunicationOPC(spsAuftraege, clientAuftraege, anlagenID, database, systemType, eventDelegate, opcServerName, opcRechnerName, datenClientID, dcName, startOffsetArrays, standardAlarming);
         }
         catch(Exception e)
         {
            LogManager.GetSingleton().ZLog("C021D", ELF.ERROR, "OpcServer.NewInstance -> {0}", e.Message);
         }
         
         return null;
      }
      
      private void OnOpcServerShutDown(string reason)
      {
         regularStartPhase = true;

         LogManager.GetSingleton().ZLog("C021E", ELF.WARNING, "OPC-Server wurde extern beendet -> {0}", reason);
         
         Disconnect();    
         running = false;
      }
      
      private Boolean TimeoutAfterStartPhaseReached()
      {
         int warteZeit = (int) (3 * ConfigManager.GetSingleton().GetParameterAsLong("TIMEOUT_OPCSERVER_RESTART_AFTER_SHUTDOWN", 5000));

         if ((DateTime.UtcNow - lastRegularStartPhaseDone) > (new TimeSpan(0, 0, 0, 0, warteZeit)))
         {
            return true;
         }
         
         return false;
      }

      public override Boolean Critical()
      {
         if (!regularStartPhase && TimeoutAfterStartPhaseReached())
         {
            return true;
         }

         return false;
      }
      
      public override void SpsUnavailable()
      {
         if (Critical())
         {
            regularStartPhase = true;
            
            Disconnect();


            running = false;
         }
      }

      public override void RegularStartDone()
      {
			
         if(regularStartPhase)
         {
            regularStartPhase = false;

            
            
            lastRegularStartPhaseDone = DateTime.UtcNow;
         }
      }
      
      public override Boolean Start()
      {
         string error = "";

         try
         {
            while (!running)
            {
               LogManager.GetSingleton().ZLog("C0220", ELF.INFO, "Verbindung zum OPC-Server herstellen: {0}/{1}", opcServerName, opcRechnerName);

               try
               {
                  Connect();
						
						if(running)
						{
							LogManager.GetSingleton().ZLog("C0221", ELF.INFO, "Verbindung zum OPC-Server erfolgreich hergestellt: {0}/{1}", opcServerName, opcRechnerName);
						}
               }
               catch (Exception e)
               {
                  error = e.Message;
               }

               if (!running)
               {
                  LogManager.GetSingleton().ZLog("C0222", ELF.WARNING, "Es konnte keine Verbindung zum OPC-Server hergestellt werden: {0}/{1}", opcServerName, opcRechnerName);
                  LogManager.GetSingleton().ZLog("C0223", ELF.WARNING, "Nächster Versuch in 5 Sekunden: {0}/{1}", opcServerName, opcRechnerName);

                  Thread.Sleep(5000);
               }
            }
         }
         catch (Exception e)
         {
            LogManager.GetSingleton().ZLog("C0224", ELF.ERROR, "Fehler beim initialisieren eines OPCClients -> {0}", e.Message);
         }

         telegramManager = TelegramManager.NewInstance(database, systemType, datenClientID, startOffsetArrays, standardAlarming);
         return telegramManager.Start();
      }

      public override void Stop()
      {
         Disconnect();

         running = false;
      }
      
      public override long ConnectDelay
      {
         get 
         {
            return ((long)(DateTime.UtcNow - lastRegularStartPhaseDone).TotalMilliseconds);
         }
      }

      public override Boolean ConnectComplete()
      {  
         //Erst nachdem alles Connected ist die aktive Gruppe aktiv schalten
         //activeGroup.SetGroupActive();
			try
			{
				subscription.ApplyChanges();
			}
			catch
			{
				return false;
			}

			return true;
      }

      
      public override Boolean AllTelegramsAvailable
      {
         get
         {
            if (!allTelegramsAvailable)
            {
               allTelegramsAvailable = CheckTelegramsAvailable();
            }

            return allTelegramsAvailable;
         }
      }
		
      protected override void ConnectTelegram(Telegram telegram)
      {
         List<String> items = new List<String>();
         string itemsForLog = "";
         int badConfigCounter = 0;
         
         try
         {
            items = new List<String>();

            

            //Diesen Teil später nur einmal machen während der Initialisierung
            foreach (string regItemKey in telegram.RegistrationItems.Keys)
            {
               items.Add(telegram.RegistrationItems[regItemKey]);

               itemsForLog += telegram.RegistrationItems[regItemKey];
               itemsForLog += "  |  ";
            }

            LogManager.GetSingleton().ZLog("C0228", ELF.INFO, "Telegram '{0}' anmelden", telegram.TeleType);
                       

            if (items.Count > 0)
            {
					LogManager.GetSingleton().ZLog("CD33F", ELF.INFO, "Registration Items (Auszug) = {0}......", itemsForLog.Substring(0, Math.Min(125, itemsForLog.Length)));

               HiresStopUhr uhr = new HiresStopUhr();

               uhr.Start();

               List<string> regItems = clientHelperAPI.RegisterNodeIds(items);

               if (regItems.Count == items.Count)
               {
                  for (int i = 0; i < regItems.Count; i++)
                  {
                     if (regItems[i] == items[i])
                     {
                        badConfigCounter++;
                        LogManager.GetSingleton().ZLog("CD341", ELF.ERROR, "Item nicht verfügbar ! {0}", regItems[i]);
                     }
                  }
               }

               if (badConfigCounter > 0)
               {
                  LogManager.GetSingleton().ZLog("CD342", ELF.ERROR, "{0} Item(s) nicht verfügbar !", badConfigCounter);
                  return;
               }

               uhr.Stop();

               LogManager.GetSingleton().ZLog("CD340", ELF.INFO, "Anmeldedauer beim OPC-UA-Server = {0} ms", uhr.PeriodMilliSeconds);
            }
         }
         catch (Exception ex)
         {
            LogManager.GetSingleton().ZLog("CD313", ELF.ERROR, "TelegramManagerTIA Start: Datenclient mit ID = {0} -> {1}", datenClientID, ex.Message);

            if (ex.Message == "BadRequestTimeout")
            {
               LogManager.GetSingleton().ZLog("CD344", ELF.ERROR, "Vermutlich läuft der OPC-UA-Server nicht oder ein Item ist falsch konfiguriert !");
            }
            return;
         }     

         try
         {

            if (telegram.SubscriptionItem.Length > 0)
            {
					LogManager.GetSingleton().ZLog("CFFF4", ELF.INFO, "Subscription Item = {0}", telegram.SubscriptionItem);

					string monitoredItemName = "DC_TIA_"  + telegram.SubscriptionItem;

               monitoredItems[monitoredItemName] = clientHelperAPI.AddMonitoredItem(subscription, telegram.SubscriptionItem, monitoredItemName, 1);

					


               if (monitoredItems[monitoredItemName] == null)
               {
                  LogManager.GetSingleton().ZLog("CD016", ELF.ERROR, "Telegram anmelden fehlgeschlagen -> \"{0}\" / {1}\nOPC-Server lieferte null für das Item !", telegram.TeleType, telegram.SubscriptionItem);
                  return;
               }

               if (!monitoredItems[monitoredItemName].Created)
               {
                  //Created ist es erst, wenn die subscription applied wird (subscription.ApplyChanges..)
                  //Das wird erst in ConnectComplete aufgerufen
                  //LogManager.GetSingleton().ZLog("CD33E", ELF.ERROR, "OPC-UA-Server konnte das Telegram nicht ertellen -> \"{0}\" / {1} !", telegram.TeleType, telegram.SubscriptionItem);
                  //return;
               }

               telegram.Available = true;
            }
            else
            {
               telegram.Available = true;
            }

            
         }
         catch (Exception e)
         {
            LogManager.GetSingleton().ZLog("CD015", ELF.ERROR, "Telegram anmelden fehlgeschlagen (AddItem) -> \"{0}\" / {1}  -> {2}", telegram.TeleType, telegram.SubscriptionItem, e.Message);
         }
      }

      private void OnNewData(MonitoredItem monitoredItem, MonitoredItemNotificationEventArgs e)
      {
			lock(DatenClient.Threads)
			{
				if(!DatenClient.Threads.ContainsKey(Thread.CurrentThread.ManagedThreadId))
				{
					DatenClient.Threads[Thread.CurrentThread.ManagedThreadId] = string.Format("{0} ({1})|OPC_THREAD", opcServerName, dcName);
				}
			}
						

			
         try
         {

            LogManager.GetSingleton().ZLog("CD30B", ELF.TELE, "EVENT HANDLER: Notification_MonitoredItem -> Echtzeit Zeitstempel: {0}", DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss:fff"));

            if (!running)
            {
               LogManager.GetSingleton().ZLog("CD30C", ELF.TELE, "NOT RUNNING");
               return;
            }

            countCallbacks++;
				         
            MonitoredItemNotification notification = e.NotificationValue as MonitoredItemNotification;

            if (notification == null)
            {
               LogManager.GetSingleton().ZLog("CD30D", ELF.TELE, "notification == null");
               return;
            }

            if (!SpsCommunicationOPC.QualityGOOD(notification.Value.StatusCode.ToString()))
            {
               LogManager.GetSingleton().ZLog("C022C", ELF.ERROR, "SpsCommunication.OnNewData Item = {1} Quality = {0} Daten werden ignoriert", notification.Value.StatusCode.ToString(), monitoredItem.StartNodeId.ToString());
               return;
            }

            //string log = "Item Name: " + monitoredItem.DisplayName + " Value: " + Utils.Format("{0}", notification.Value.WrappedValue.ToString()) + " Status Code: " + Utils.Format("{0}", notification.Value.StatusCode.ToString()) + " Source timestamp: " + notification.Value.SourceTimestamp.ToString() + " Server timestamp: " + notification.Value.ServerTimestamp.ToString();
            //LogManager.GetSingleton().ZLog("C022B", ELF.TELE, "Subscription Item empfangen: {0}", log);
         
         
            Telegram telegram = null;

				/*
				if(!monitoredItem.StartNodeId.ToString().ToLower().Contains("watchdog"))
				{
					LogManager.GetSingleton().ZLog("XXXXXX", ELF.ERROR, "Neue Daten für -> {0}", monitoredItem.DisplayName);
				}
				*/
				
         
            try
            {
               telegram = telegramManager.NewTelegramInstanceByItemID(monitoredItem.StartNodeId.ToString());
            }
            catch (Exception e3)
            {
               LogManager.GetSingleton().ZLog("CD30A", ELF.ERROR, "Notification_MonitoredItem POS1 -> {0}", e3.Message);
            }

            if (telegram == null)
            {
               LogManager.GetSingleton().ZLog("C022D", ELF.WARNING, "Empfangenes Telegram kann nicht zugeordnet werden -> Node = {0}", monitoredItem.StartNodeId.ToString());
            }
            else
            {
               telegram.SetRxData(notification.Value.WrappedValue.Value, notification.Value.StatusCode.ToString(), notification.Value.SourceTimestamp);
            }

            LogManager.GetSingleton().ZLog("C022E", ELF.TELE, "OPC RX {0}", monitoredItem.StartNodeId.ToString());

            EvtTelegram evt = Evt.NewTelegramEvt(telegram);

            eventDelegate(evt);  //Siehe DatenClient.NotifyTelegram
         }
         catch(Exception eGes)
         {
            LogManager.GetSingleton().ZLog("CFFF2", ELF.ERROR, "OnNewData Exception ! Message = {0}", eGes.Message);
         }
      }

      
      
      private void Disconnect()
      {
         try
         {
            LogManager.GetSingleton().ZLog("CFFFA", ELF.INFO, "OpcServer.Disconnect -> {0}", this.dcName);

            if (subscription != null)
            {
               clientHelperAPI.RemoveSubscription(subscription);

               subscription.Delete(true);
            }

            clientHelperAPI.Disconnect();
         }
         catch (Exception e)
         {
            LogManager.GetSingleton().ZLog("C0405", ELF.ERROR, "OpcServer.Disconnect -> {0}", e.Message);
         }         
      }

      private void Connect()
      {
         monitoredItems = new Dictionary<string, MonitoredItem>();


         clientHelperAPI = new UAClientHelperAPI();
         clientHelperAPI.ItemChangedNotification += new MonitoredItemNotificationEventHandler(OnNewData);


         LogManager.GetSingleton().ZLog("CD300", ELF.INFO, "Endpunkt zum OPC-UA-Server suchen mit folgender Information:{0}", opcServerName);


         string applicationName = "";
         string securityPolicy = "";

         
         try
         {
            ApplicationDescriptionCollection servers = clientHelperAPI.FindServers(opcServerName);
            foreach (ApplicationDescription ad in servers)
            {
               foreach (string url in ad.DiscoveryUrls)
               {
                  EndpointDescriptionCollection endpoints = clientHelperAPI.GetEndpoints(url);
                  foreach (EndpointDescription ep in endpoints)
                  {
                     securityPolicy = ep.SecurityPolicyUri.Remove(0, 42);

                     if (securityPolicy.ToLower().Contains("none"))
                     {
                        applicationName = ad.ApplicationName.Text;
                        theEndpoint = ep;
                        LogManager.GetSingleton().ZLog("CD301", ELF.INFO, "Gefundener Endpunkt:{0}", theEndpoint.EndpointUrl);
                        break;
                     }
                  }
               }
            }
         }
         catch (Exception ex)
         {
				if(theEndpoint != null)
				{
					LogManager.GetSingleton().ZLog("CD305", ELF.ERROR, "Fehler beim Herstellen einer Verbindung zum OPC-UA-Server (1): {0} -> {1}", theEndpoint.EndpointUrl, ex.Message);
				}
				else
				{
					LogManager.GetSingleton().ZLog("D000F", ELF.ERROR, "Fehler beim Herstellen einer Verbindung zum OPC-UA-Server (2): {0}", ex.Message);
				}
            return;
         }

         LogManager.GetSingleton().ZLog("CD325", ELF.INFO, "Endpunkt zum OPC-UA-Server gefunden:{0}", theEndpoint.SecurityPolicyUri);

         try
         {
            clientHelperAPI.Connect(theEndpoint, false, "", "");
            session = clientHelperAPI.Session;   
            

            session.KeepAlive += Session_KeepAlive;


            string log = "[" + applicationName + "] " + " [" + theEndpoint.SecurityMode + "] " + " [" + securityPolicy + "] " + " [" + theEndpoint.EndpointUrl + "]";
            LogManager.GetSingleton().ZLog("CD303", ELF.INFO, "Verbindung zum Endpunkt erfolgreich hergestellt:{0}", log);

            running = true;
         }
         catch (Exception ex2)
         {
            LogManager.GetSingleton().ZLog("CD304", ELF.ERROR, "Fehler beim herstellen einer Verbindung zum OPC-UA-Server: {0} -> {1}", theEndpoint.EndpointUrl, ex2.Message);
         }

         LogManager.GetSingleton().ZLog("CD306", ELF.INFO, "SubscribeIntervall hardcodiert auf 0 ms. Dies entspricht in der Entwicklungsumgebun einem CurrentPublishingInterval von 30 ms");
         subscription = clientHelperAPI.Subscribe(0);     
			
			//double test = subscription.CurrentPublishingInterval;

			/*
			CG: 30.07.2020
			Das Subscriber-Intervall des Clients kann laut Herrn Mehrbrodt ruhig auf 0 gestellt werden.
			Der Server bestimmt sowieso dessen Update-Rate.
			Ausserdem ist im weiteren Verlauf sowieso folgendes Intervall von Bedeutung:
			double test = subscription.CurrentPublishingInterval;
			Das CurrentPublishingInterval ist immer größer als das Vorgeschlagene. Aus 0 wird z.B. 30 ms und aus 100 ms werden 120 ms.
			Beim DC dürfte alles unkritisch sein, da sowieso nur drei Items aktiv sind: Watchdog, Störänderung, Datenänderung
			https://documentation.unified-automation.com/uasdkdotnet/2.6.0/html/classUnifiedAutomation_1_1UaClient_1_1Subscription.html#a3212a2bd064f54354cd1c45ce85f4e0a
			*/
      }

      private void Session_KeepAlive(Session session, KeepAliveEventArgs e)
      {
         
      }

      public static Boolean QualityGOOD(string status)
      {
         if (status.ToUpper().Contains("GOOD"))
         {
            return true;
         }

         return false;
      }

   
      public override void SendTelegram(Telegram telegram)
      {
         if(!running)
         {
            return;
         }

        
         HiresStopUhr watch = new HiresStopUhr();

         watch.Start();

         LogManager.GetSingleton().ZLog("C0232", ELF.TELE, "TX {0} ->[{1}],[{2}],[{3}]", telegram.TeleType, telegram.SendVisuData, telegram.Quality, telegram.TimeStamp.ToString("dd.MM.yy HH:mm:ss.fff"));

         
         List<string> nodeIDs = new List<string>();
         List<string> values = new List<string>();

         
         foreach (string key in telegram.RegistrationItems.Keys)
         {
            nodeIDs.Add(telegram.RegistrationItems[key]);
            values.Add(telegram.RegistrationValues[key]);
         }

         try
         {
            if (nodeIDs.Count == 0)
            {
               LogManager.GetSingleton().ZLog("CD347", ELF.ERROR, "Keine Items zum Schreiben in die SPS gefunden. Telegram = {0}", telegram.TeleType);

               if (telegram.TeleType == "M")
               {
                  LogManager.GetSingleton().ZLog("CD348", ELF.ERROR, "Dies kann durch ein falsch konfiguriertes UpdateKriterium passieren.");
               }

               return;
            }
            else
            {
               if((telegram.TeleType == "F") || (telegram.TeleType == "M"))
               {
               }

               string ret = clientHelperAPI.WriteValues(values, nodeIDs, telegram.TeleType);

               if (ret != "OK")
               {
                  LogManager.GetSingleton().ZLog("CD345", ELF.ERROR, ret);
                  telegram.ErrorText = ret;
               }
            }
         }
         catch (Exception ex)
         {
            LogManager.GetSingleton().ZLog("CD30E", ELF.ERROR, "SpsCommunicatioOPC.Write fehlgeschlagen: Item={0} Error={1}", telegram.SubscriptionItem, ex.Message);

            string log = "Eines der folgenden Items konnte nicht geschrieben werden:";
            log += System.Environment.NewLine;
            foreach(string nodeID in nodeIDs)
            {
               log += nodeID;
               log += System.Environment.NewLine;
            }

            LogManager.GetSingleton().ZLog("CD34F", ELF.ERROR, "{0}", log);
         }

         watch.Stop();
      }
      
      public override void ForceTelegram(string teleType, object tag)
      {
         if(!running)
         {
            return;
         }

         string errorText = "";


         int anzahl_Störeinträge = -1;

         HiresStopUhr stopUhr = new HiresStopUhr();

         Telegram telegram = telegramManager.NewTelegramInstance(teleType);

         if(telegram == null)
         {
            LogManager.GetSingleton().ZLog("C0233", ELF.ERROR, "OpcServer.ForceTelegram() -> Angefordertes Telegram existiert nicht -> {0}", teleType);
            return;
         }

         if ((tag == null) && (teleType == "E"))
         {
            LogManager.GetSingleton().ZLog("C0234", ELF.ERROR, "OpcServer.ForceTelegram() -> Telegram E benötigt Telegram D -> {0}", teleType);
            return;
         }
         
         
         //Wenn "T" dann erst Anzahl separat holen und dann erst die entsprechende Anzahl an Störungen
         if (teleType == "T")
         {
				((TelegramT)telegram).IndexTelegramS = ((TelegramS)tag).IndexTelegramS;

            List<String> items = new List<String>();
            List<String> keys = new List<String>();
            List<object> values = new List<object>();

            items.Add(telegram.RegistrationItems["T.Anzahl_Störeinträge"]);

            try
            {
               values = clientHelperAPI.ReadValues(items);
               anzahl_Störeinträge = Convert.ToInt32(values[0]);

               LogManager.GetSingleton().ZLog("CD316", ELF.INFO, "Anzahl aktueller Störungen = {0}", anzahl_Störeinträge);
               
               stopUhr = new HiresStopUhr();
               stopUhr.Start();

               items = new List<String>();

               
               //Diesen Teil später nur einmal machen während der Initialisierung
               foreach (string regItemKey in telegram.RegistrationItems.Keys)
               {
                  if (regItemKey == "T.Anzahl_Störeinträge")
                  {
                     continue;
                  }

                  string currentIndexString = regItemKey.Substring(regItemKey.Length - 3, 3);
                  int currentIndex = Convert.ToInt32(currentIndexString);

						int von = currentIndex;
						int bis = anzahl_Störeinträge + von;

						//INDEX 18112020
                  if (von <= (anzahl_Störeinträge))
						//if (von <= 40)
						//if (von < bis)
                  {
							//Nur soviele Störungen holen wie anstehen....
                     keys.Add(regItemKey);
                     items.Add(telegram.RegistrationItems[regItemKey]);
                  }
               }

					LogManager.GetSingleton().ZLog("D0010", ELF.INFO, "Anzahl angefragter Stör-Items = {0}", keys.Count);

               stopUhr.Stop();

               stopUhr = new HiresStopUhr();
               stopUhr.Start();

					try
					{
						if(anzahl_Störeinträge > 0)
						{
							//Nur lesen wenn es Störungen gibt !
							values = clientHelperAPI.ReadValues(items);

							stopUhr.Stop();
							LogManager.GetSingleton().ZLog("CD318", ELF.INFO, "{0} Alarme in {1} ms vom UA-Server gelesen", anzahl_Störeinträge, stopUhr.PeriodMilliSeconds);
						}
					}
					catch(Exception e3)
					{
						string logInfo = "";

						foreach(string key in items)
						{
							logInfo += key;
							logInfo += Environment.NewLine;
						}

						LogManager.GetSingleton().ZLog("D000E", ELF.ERROR, "Fehler beim Lesen der Alarme vom OPC-Server -> {0}\nItems die gelesen werden sollten:\n{1}", e3.Message, logInfo);
						
						throw new Exception("Abbruch wegen Fehler: D000E");
					}

               keys.Insert(0, "T.Anzahl_Störeinträge");
               values.Insert(0, anzahl_Störeinträge.ToString());

               Dictionary<string, object> keyValues = new Dictionary<string, object>();

               for(int i = 0; i < keys.Count; i++)
               {
                  keyValues[keys[i]] = values[i];
               }

               telegram.SetRxData(keyValues, "Good", DateTime.Now);               

            }
            catch (Exception ex)
            {
               LogManager.GetSingleton().ZLog("CD319", ELF.ERROR, "SpsCommunication.ForceTelegram() -> Telegram = {0} -> {1}", teleType, ex.Message);
            }

				//Weiter bei:
				//public override void NotifyTelegramT(Telegram telegram)
         }

         if (teleType == "E")
         {
            ((TelegramE)telegram).IndexTelegramD = ((TelegramD)tag).IndexTelegramD;

            List<String> items = new List<String>();
            List<String> keys = new List<String>();
            List<object> values = new List<object>();

            items.Add(telegram.RegistrationItems["E.ID"]);

            try
            {
               values = clientHelperAPI.ReadValues(items);
               int auftragsNummer = Convert.ToInt32(values[0]);

               ((TelegramE)telegram).Auftragsnummer = auftragsNummer;

               LogManager.GetSingleton().ZLog("CD33B", ELF.INFO, "Auftragsnummer={0}", auftragsNummer);

					if(auftragsNummer == 105)
					{
					}

               stopUhr = new HiresStopUhr();
               stopUhr.Start();

               items = new List<String>();


               //Hier die RegistrationItems dynamisch in das TelegramE einpflanzen......
               Auftrag auftrag = null;

               try
               {
                  auftrag = spsAuftraege[Auftrag.AuftragPrimaryKey(auftragsNummer, anlagenID)];
               }
               catch
               {                  
               }

               if (auftrag == null)
               {
                  //Auftrag inaktiv
                  LogManager.GetSingleton().ZLog("CD343", ELF.WARNING, "Auftragsnummer {0} mit AnlagenID {1} nicht konfiguriert oder inaktiv ! -> Key = {2}", auftragsNummer, anlagenID, Auftrag.AuftragPrimaryKey(auftragsNummer, anlagenID));
						
						string auftragLog = "";
						
						foreach(string key in spsAuftraege.Keys)
						{
							auftragLog += Environment.NewLine;
							auftragLog += Auftrag.AuftragPrimaryKey(spsAuftraege[key].AuftragsNummer, spsAuftraege[key].AnlagenID);							
						}

						LogManager.GetSingleton().ZLog("D0005", ELF.WARNING, "Anzahl Auftraege = {1} Vorhandene Auftraege = (Auftragsnummer:AnlagenID) -> {0}", auftragLog, spsAuftraege.Keys.Count);

                  return;
               }

               foreach (long key in auftrag.AuftragDetails.Keys)
               {
                  AuftragsDetail ad = auftrag.AuftragDetails[key];
                  telegram.RegistrationItems[key.ToString()] = ad.UaItem;
               }



               //Diesen Teil später nur einmal machen während der Initialisierung
               foreach (string regItemKey in telegram.RegistrationItems.Keys)
               {
                  if (regItemKey == "E.ID")
                  {
                     continue;
                  }

                  if (telegram.RegistrationItems[regItemKey].Contains("@INDEX@"))
                  {
                     continue;
                  }

                  //Items müssen beim erstellen des Auftrags noch in in die registrationItems aufgenommen werden !!!!
                  keys.Add(regItemKey);
                  items.Add(telegram.RegistrationItems[regItemKey]);
               }

               try
               {
                  if(items.Count > 0)
                  {
                     //Bei Leseaufträgen muss man nur die Auftragsnummer lesen.
                     //Das findet weiter oben schon statt.
                     //Also gibt es hier nichts mehr zu tun.....

							

                     values = clientHelperAPI.ReadValues(items);
                  }
               }
               catch(Exception e)
               {
                  LogManager.GetSingleton().ZLog("CD34C", ELF.ERROR, "Fehler beim Lesen der Items vom OPC-UA-Server.");

                  throw new Exception(string.Format("ForceTelegram -> Exception weitergeleitet -> {0}", e.Message));
               }

               Dictionary<string, object> keyValues = new Dictionary<string, object>();
               
               for (int i = 0; i < keys.Count; i++)
               {
                  keyValues[keys[i]] = values[i];
               }

               telegram.SetRxData(keyValues, "Good", DateTime.Now);

               stopUhr.Stop();

               LogManager.GetSingleton().ZLog("CD339", ELF.INFO, "Daten von Auftragsnummer {0} in {1} ms vom UA-Server gelesen", auftragsNummer, stopUhr.PeriodMilliSeconds);

            }
            catch (Exception ex)
            {
               LogManager.GetSingleton().ZLog("CD338", ELF.ERROR, "SpsCommunication.ForceTelegram() -> Telegram = {0} -> {1}", teleType, ex.Message);

               string logItems = "\n";
               foreach (string itemName in items)
               {
                  logItems += itemName + Environment.NewLine; 
               }

               errorText = logItems;

               LogManager.GetSingleton().ZLog("CD346", ELF.ERROR, "Mind. eins folgender Items nicht lesbar:\n{0}", logItems);
               
            }
         }



         if (teleType[0] == 'A')
         {
            List<String> items = new List<String>();
            List<String> keys = new List<String>();
            List<object> values = new List<object>();

            try
            {
               long auftragsNummer = ((TelegramA)telegram).Auftragsnummer;
               LogManager.GetSingleton().ZLog("CD32B", ELF.INFO, "Auftragsnummer={0}", auftragsNummer);

               
               stopUhr = new HiresStopUhr();
               stopUhr.Start();

               items = new List<String>();


               //Hier die RegistrationItems dynamisch in das TelegramA einpflanzen......
               Auftrag auftrag = clientAuftraege[Auftrag.AuftragPrimaryKey(auftragsNummer, anlagenID)];

               foreach (long key in auftrag.AuftragDetails.Keys)
               {
                  AuftragsDetail ad = auftrag.AuftragDetails[key];
                  telegram.RegistrationItems[key.ToString()] = ad.UaItem;
               }



               foreach (string regItemKey in telegram.RegistrationItems.Keys)
               {
                  keys.Add(regItemKey);
                  items.Add(telegram.RegistrationItems[regItemKey]);
               }

               values = clientHelperAPI.ReadValues(items);


               Dictionary<string, object> keyValues = new Dictionary<string, object>();

               for (int i = 0; i < keys.Count; i++)
               {
                  keyValues[keys[i]] = values[i];
               }

               telegram.SetRxData(keyValues, "Good", DateTime.Now);

               stopUhr.Stop();

               LogManager.GetSingleton().ZLog("CD332", ELF.INFO, "Daten von Auftragsnummer {0} in {1} ms vom UA-Server gelesen", auftragsNummer, stopUhr.PeriodMilliSeconds);

            }
            catch (Exception ex)
            {
               LogManager.GetSingleton().ZLog("CD333", ELF.ERROR, "SpsCommunication.ForceTelegram() -> Telegram = {0} -> {1}", teleType, ex.Message);
            }
         }


         try
         {
            //Siehe 
            //public override void NotifyTelegramE(TelegramE telegramE)
            //public override void NotifyTelegramT(Telegram telegram)

            telegram.ErrorText = errorText; //Wichtig, um später der SPS eine Quittung schicken zu können
            EvtTelegram evt = Evt.NewTelegramEvt(telegram);
            eventDelegate(evt);        
         }
         catch(Exception e)
         {
            LogManager.GetSingleton().ZLog("C0235", ELF.ERROR, "SpsCommunication.ForceTelegram() -> Exception beim Lesen des Telegrams: {0}  -> {1} telegram.ItemID = {2}", teleType, e.Message, telegram.SubscriptionItem);
            return;
         }
      }

		public override Boolean MonitoredItemsCreated(Boolean mitLoggen)
		{
			Boolean allItemsCreated = true;

			foreach(string monitoredItemName in monitoredItems.Keys)
			{
				MonitoredItem mi = monitoredItems[monitoredItemName];

				if(!mi.Created)
				{
					allItemsCreated = false;

					if(mitLoggen)
					{
						LogManager.GetSingleton().ZLog("CFFFD", ELF.WARNING, "Aktives Telegramm {0} noch nicht verfügbar !", monitoredItemName);
					}
				}
				else
				{
					if(mitLoggen)
					{						
						LogManager.GetSingleton().ZLog("CFFFE", ELF.INFO, "Aktives Telegramm {0} verfügbar !", monitoredItemName);
					}
				}
			}

			if(allItemsCreated)
			{
				LogManager.GetSingleton().ZLog("CFFFB", ELF.INFO, "SpsCommunication.MonitoredItemsCreated -> Alle aktiven Telegramme created. ");
			}
			

			return allItemsCreated;
		}

      public override Telegram NewTelegramInstance(string teleType)
      {
         return telegramManager.NewTelegramInstance(teleType);
      }

      public override Telegram GetReferenceInstance(string teleType)
      {
         return telegramManager.GetReferenceInstance(teleType);
      }
   }
}
