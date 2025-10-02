using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;

namespace DatenClieNT
{
   public class TelegramManagerTIA : TelegramManager
   {
		protected int startOffsetArrays = 0;
		protected long maxAlarme = 200;
		protected Boolean standardAlarming = true;

      public TelegramManagerTIA(string database, long datenClientID, int startOffsetArrays, Boolean standardAlarming)
         : base(database, datenClientID)
      {
			this.startOffsetArrays = startOffsetArrays;
			this.standardAlarming = standardAlarming;
      }

      private Dictionary<string, string> GetTelegramItems(string teleType, TelegramConnectType telegramConnectType)
      {
         Dictionary<string, string> telegramItems = new Dictionary<string, string>();
         string statement = "";

         try
         {
            if (telegramConnectType == TelegramConnectType.subscription)
            {
               statement = string.Format("select * from DC_DatenclientsTIA_Items where ConnectType = 'Subscription' and Telegram = '{0}' and Datenclient_ID = {1}", teleType, datenClientID);
            }
            else if (telegramConnectType == TelegramConnectType.registration)
            {
               statement = string.Format("select * from DC_DatenclientsTIA_Items where ConnectType = 'Registration' and Telegram = '{0}' and Datenclient_ID = {1}", teleType, datenClientID);
            }

            SqlRealtimeSimpleQuery query = new SqlRealtimeSimpleQuery(database, statement);

            if (query.QueryResult != null)
            {
               foreach (Dictionary<string, object> prm in query.QueryResult)
               {
                  try
                  {
                     string opc_Item = Tools.GetString(prm, "OPC_Item");
                     string dc_Item = string.Format("{0}.{1}", teleType, Tools.GetString(prm, "DC_Item"));

                     telegramItems[dc_Item] = opc_Item;
                  }
                  catch (Exception e)
                  {
                     LogManager.GetSingleton().ZLog("CD263", ELF.ERROR, "Error in GetTelegramItems -> {0}", e.Message);
                     return null;
                  }
               }
            }
         }
         catch (Exception e)
         {

            LogManager.GetSingleton().ZLog("CD264", ELF.ERROR, "Error in ItemsOfWatchdog -> {0} Statement = {1}", e.Message, statement);
            return null;
         }

         return telegramItems;
      }


      private Dictionary<string, string> GetTelegramAItems(long auftragsID)
      {
         Dictionary<string, string> telegramItems = new Dictionary<string, string>();
         string statement = "";

         try
         {
            statement = string.Format("select * from DC_AuftragsDetailsTIA where Auftrags_ID = {0}", auftragsID);

            SqlRealtimeSimpleQuery query = new SqlRealtimeSimpleQuery(database, statement);

            if (query.QueryResult != null)
            {
               foreach (Dictionary<string, object> prm in query.QueryResult)
               {
                  try
                  {
                     string opc_Item = Tools.GetString(prm, "UA_Item");
                     string dc_Item = string.Format("{0}", Tools.GetLong(prm, "ID").ToString());

                     telegramItems[dc_Item] = opc_Item;
                  }
                  catch (Exception e)
                  {
                     LogManager.GetSingleton().ZLog("CD330", ELF.ERROR, "Error in GetTelegramAItems -> {0}", e.Message);
                     return null;
                  }
               }
            }
         }
         catch (Exception e)
         {

            LogManager.GetSingleton().ZLog("CD331", ELF.ERROR, "Error in ItemsOfWatchdog -> {0} Statement = {1}", e.Message, statement);
            return null;
         }

         return telegramItems;
      }

      private void GetParameter(string param, out bool value)
      {
         value = false;

         try
         {
            SqlRealtimeSimpleQuery query = new SqlRealtimeSimpleQuery(database, string.Format("select * from DC_DatenclientsTIA where id = {0}", datenClientID));

            if (query.QueryResult != null)
            {
               Dictionary<string, object> prm = query.QueryResult[0];

               try
               {
                  value = Tools.GetBoolean(prm, param);
               }
               catch
               {
               }
            }
         }
         catch (Exception ee)
         {
            LogManager.GetSingleton().ZLog("CD30F", ELF.ERROR, "GetParameter -> Start() -> {0}", ee.Message);
         }

      }

		private long GetPramaterMaxAlarme(int defaultValue)
      {
         long maxAlarme = 200;

         try
         {
            SqlRealtimeSimpleQuery query = new SqlRealtimeSimpleQuery(database, string.Format("select * from DC_DatenclientsTIA where id = {0}", datenClientID));

            if (query.QueryResult != null)
            {
               Dictionary<string, object> prm = query.QueryResult[0];

               try
               {
                  maxAlarme = Tools.GetLong(prm, "MaxAlarme");
               }
               catch
               {
               }
            }
         }
         catch (Exception ee)
         {
            LogManager.GetSingleton().ZLog("D0004", ELF.ERROR, "GetParameter -> Start() -> {0}", ee.Message);
         }

			return maxAlarme;
      }

      public override Boolean Start()
      {
         base.Start();

         GetParameter("AktivAlarming", out Boolean aktivAlarming);
         GetParameter("AktivDatenEmpfang", out Boolean aktivDatenEmpfang);
			maxAlarme = GetPramaterMaxAlarme(200);

         Dictionary<string, string> registrationItems = null;
         Dictionary<string, string> subscriptionItems = null; //Nur eins......Das ist dann auch der Handle....


         //WATCHDOG -----------------------------------------------------------------------------------
         string teleType = "W";
         string nameOfSubscriptionItem = "W.Watchdog";
         string itemNameOfSubscriptionItem = "";


         try
         {
            subscriptionItems = GetTelegramItems(teleType, TelegramConnectType.subscription);
            registrationItems = GetTelegramItems(teleType, TelegramConnectType.registration);

            if (subscriptionItems == null || subscriptionItems.Count != 1)
            {
               LogManager.GetSingleton().ZLog("CD307", ELF.ERROR, "Konfigurationsfehler des Watchdogs: Datenclient mit ID = {0} -> SubscriptionItem nicht definiert ! (In der Regel: Watchdog)", datenClientID);
               return false;
            }

            if (registrationItems == null)
            {
               LogManager.GetSingleton().ZLog("CD308", ELF.ERROR, "Konfigurationsfehler des Watchdogs: Datenclient mit ID = {0} -> Keine Registration-Items", datenClientID);
               return false;
            }

            foreach (string key in subscriptionItems.Keys)
            {
               if (!key.Contains(nameOfSubscriptionItem))
               {
                  LogManager.GetSingleton().ZLog("CD309", ELF.ERROR, "Konfigurationsfehler des Watchdogs: Datenclient mit ID = {0} -> Der zwingend notwendige Subscription-Name 'W.Watchdog' ist nicht konfiguriert !", datenClientID);
               }

               itemNameOfSubscriptionItem = subscriptionItems[key];
               break;
               //Darf sowieso nur einer hier drin sein
            }

            teleType = "W";
            TelegramW telegramW = (TelegramW)Telegram.NewInstance(itemNameOfSubscriptionItem, registrationItems, teleType);

            telegramsByTeleType[teleType] = telegramW;
            telegramsByItemID[itemNameOfSubscriptionItem] = telegramW;
         }
         catch (Exception ew)
         {
            LogManager.GetSingleton().ZLog("CD32E", ELF.ERROR, "Konfigurationsfehler Telegram W: Datenclient mit ID = {0} -> {1}", datenClientID, ew.Message);
         }

         if (aktivAlarming)
         {
            //Störänderung (Telegram S) -----------------------------------------------------------------------------------
            teleType = "S";
            nameOfSubscriptionItem = "S.Änderung_Störeinträge";
            itemNameOfSubscriptionItem = "";


            try
            {
               subscriptionItems = GetTelegramItems(teleType, TelegramConnectType.subscription);
               registrationItems = GetTelegramItems(teleType, TelegramConnectType.registration);

               if (subscriptionItems == null || subscriptionItems.Count != 1)
               {
                  LogManager.GetSingleton().ZLog("CD310", ELF.ERROR, "Konfigurationsfehler Telegram S: Datenclient mit ID = {0} -> SubscriptionItem nicht definiert !", datenClientID);
                  return false;
               }

               if (registrationItems == null)
               {
                  LogManager.GetSingleton().ZLog("CD311", ELF.ERROR, "Konfigurationsfehler Telegram S: Datenclient mit ID = {0} -> Keine Registration-Items", datenClientID);
                  return false;
               }

               foreach (string key in subscriptionItems.Keys)
               {
                  if (!key.Contains(nameOfSubscriptionItem))
                  {
                     LogManager.GetSingleton().ZLog("CD317", ELF.ERROR, "Konfigurationsfehler Telegram S: Datenclient mit ID = {0} -> Der zwingend notwendige Subscription-Name 'S.Änderung_Störeinträge' ist nicht konfiguriert !", datenClientID);
                  }

                  itemNameOfSubscriptionItem = subscriptionItems[key];
                  break;
                  //Darf sowieso nur einer hier drin sein
               }

               //Telegram "S" anlegen

               teleType = "S";
               TelegramS telegramS = (TelegramS)Telegram.NewInstance(itemNameOfSubscriptionItem, registrationItems, teleType);

               telegramsByTeleType[teleType] = telegramS;
               telegramsByItemID[itemNameOfSubscriptionItem] = telegramS;
            }
            catch (Exception ex)
            {
               LogManager.GetSingleton().ZLog("CD312", ELF.ERROR, "TelegramManagerTIA Start: Datenclient mit ID = {0} -> {1}", datenClientID, ex.Message);
            }


            //Störungen (Telegram T) -----------------------------------------------------------------------------------
            teleType = "T";
            nameOfSubscriptionItem = "";
            itemNameOfSubscriptionItem = "";


            try
            {
               subscriptionItems = GetTelegramItems(teleType, TelegramConnectType.subscription);
               Dictionary<string, string> registrationItemsTEMP = GetTelegramItems(teleType, TelegramConnectType.registration);

               //Anzahl_Störeinträge sind nur einmal (also ohne Index)

               foreach (string regItemKey in registrationItemsTEMP.Keys)
               {
                  string regItemValue = registrationItemsTEMP[regItemKey];

                  if (regItemKey == "T.Anzahl_Störeinträge")
                  {
                     registrationItems[regItemKey] = regItemValue;
                     continue;
                  }

						int von = startOffsetArrays;
						int bis = (int)maxAlarme + startOffsetArrays;


						//INDEX 18112020
                  for (int i = von; i < bis; i++) //Bei TIA bzw. IROPA können max. 200 Störungen gleichzeitig anstehen
                  {
                     string newRegItemKey = string.Format("{0}_{1}", regItemKey, i.ToString().PadLeft(3, '0'));

                     registrationItems[newRegItemKey] = regItemValue.Replace("@INDEX@", i.ToString()); ;


                  }
               }

               if (subscriptionItems == null || subscriptionItems.Count != 0)
               {
                  LogManager.GetSingleton().ZLog("CD31A", ELF.ERROR, "Konfigurationsfehler Telegram T: Datenclient mit ID = {0} -> Telegram T darf kein SubscriptionItem haben !", datenClientID);
                  return false;
               }

               if (registrationItems == null)
               {
                  LogManager.GetSingleton().ZLog("CD314", ELF.ERROR, "Konfigurationsfehler Telegram T: Datenclient mit ID = {0} -> Keine Registration-Items", datenClientID);
                  return false;
               }


               teleType = "T";
               TelegramT telegramT = (TelegramT)Telegram.NewInstance(itemNameOfSubscriptionItem, registrationItems, teleType);

               telegramsByTeleType[teleType] = telegramT;
               telegramsByItemID[itemNameOfSubscriptionItem] = telegramT;
            }
            catch (Exception ex)
            {
               LogManager.GetSingleton().ZLog("CD315", ELF.ERROR, "TelegramManagerTIA Start: Datenclient mit ID = {0} -> {1}", datenClientID, ex.Message);
            }



            //Antwort Störungen (Telegram U) -----------------------------------------------------------------------------------
            teleType = "U";
            nameOfSubscriptionItem = "";
            itemNameOfSubscriptionItem = "";


            try
            {
               subscriptionItems = GetTelegramItems(teleType, TelegramConnectType.subscription);
               registrationItems = GetTelegramItems(teleType, TelegramConnectType.registration);

               if (subscriptionItems.Count != 0)
               {
                  LogManager.GetSingleton().ZLog("CD323", ELF.ERROR, "Konfigurationsfehler Telegram U: Datenclient mit ID = {0} -> Telegram U darf kein SubscriptionItem haben !", datenClientID);
                  return false;
               }

               if (registrationItems.Count != 1)
               {
                  LogManager.GetSingleton().ZLog("CD321", ELF.ERROR, "Konfigurationsfehler Telegram U: Datenclient mit ID = {0} -> Kein Registration-Item", datenClientID);
                  return false;
               }


               teleType = "U";
               TelegramU telegramU = (TelegramU)Telegram.NewInstance(itemNameOfSubscriptionItem, registrationItems, teleType);

               telegramsByTeleType[teleType] = telegramU;
               telegramsByItemID[itemNameOfSubscriptionItem] = telegramU;
            }
            catch (Exception ex)
            {
               LogManager.GetSingleton().ZLog("CD322", ELF.ERROR, "TelegramManagerTIA Start: Datenclient mit ID = {0} -> {1}", datenClientID, ex.Message);
            }
         }


         if (aktivDatenEmpfang)
         {
            //Datenveränderung (Telegram D) -----------------------------------------------------------------------------------
            teleType = "D";
            nameOfSubscriptionItem = "D.Veränderung";
            itemNameOfSubscriptionItem = "";


            try
            {
               subscriptionItems = GetTelegramItems(teleType, TelegramConnectType.subscription);
               registrationItems = GetTelegramItems(teleType, TelegramConnectType.registration);

               if (subscriptionItems == null || subscriptionItems.Count != 1)
               {
                  LogManager.GetSingleton().ZLog("CD326", ELF.ERROR, "Konfigurationsfehler Telegram D: Datenclient mit ID = {0} -> SubscriptionItem nicht definiert !", datenClientID);
                  return false;
               }

               if (registrationItems == null)
               {
                  LogManager.GetSingleton().ZLog("CD327", ELF.ERROR, "Konfigurationsfehler Telegram D: Datenclient mit ID = {0} -> Keine Registration-Items", datenClientID);
                  return false;
               }

               foreach (string key in subscriptionItems.Keys)
               {
                  if (!key.Contains(nameOfSubscriptionItem))
                  {
                     LogManager.GetSingleton().ZLog("CD328", ELF.ERROR, "Konfigurationsfehler Telegram D: Datenclient mit ID = {0} -> Der zwingend notwendige Subscription-Name 'D.Veränderung' ist nicht konfiguriert !", datenClientID);
                  }

                  itemNameOfSubscriptionItem = subscriptionItems[key];
                  break;
                  //Darf sowieso nur einer hier drin sein
               }

               TelegramD telegramD = (TelegramD)Telegram.NewInstance(itemNameOfSubscriptionItem, registrationItems, teleType);

               telegramsByTeleType[teleType] = telegramD;
               telegramsByItemID[itemNameOfSubscriptionItem] = telegramD;
            }
            catch (Exception ex)
            {
               LogManager.GetSingleton().ZLog("CD329", ELF.ERROR, "TelegramManagerTIA Start: Datenclient mit ID = {0} -> {1}", datenClientID, ex.Message);
            }

            try
            {
               teleType = "E";
               nameOfSubscriptionItem = "";
               itemNameOfSubscriptionItem = "";

               subscriptionItems = GetTelegramItems(teleType, TelegramConnectType.subscription);
               registrationItems = GetTelegramItems(teleType, TelegramConnectType.registration);


               if (subscriptionItems.Count != 0)
               {
                  LogManager.GetSingleton().ZLog("CD32C", ELF.ERROR, "Konfigurationsfehler Telegram E: Datenclient mit ID = {0} -> SubscriptionItem zuviel definiert !", datenClientID);
                  return false;
               }

               TelegramE telegramE = (TelegramE)Telegram.NewInstance(itemNameOfSubscriptionItem, registrationItems, teleType);

               telegramsByTeleType[teleType] = telegramE;
               //telegramsByItemID[itemNameOfSubscriptionItem] = telegramE;
            }
            catch (Exception eE)
            {
               LogManager.GetSingleton().ZLog("CD32F", ELF.ERROR, "TelegramManagerTIA Start: Datenclient mit ID = {0} -> {1}", datenClientID, eE.Message);
            }


            try
            {
               teleType = "F";
               nameOfSubscriptionItem = "";
               itemNameOfSubscriptionItem = "";

               subscriptionItems = GetTelegramItems(teleType, TelegramConnectType.subscription);
               registrationItems = GetTelegramItems(teleType, TelegramConnectType.registration);


               if (subscriptionItems.Count != 0)
               {
                  LogManager.GetSingleton().ZLog("CD334", ELF.ERROR, "Konfigurationsfehler Telegram E: Datenclient mit ID = {0} -> SubscriptionItem zuviel definiert !", datenClientID);
                  return false;
               }

               TelegramF telegramF = (TelegramF)Telegram.NewInstance(itemNameOfSubscriptionItem, registrationItems, teleType);

               telegramsByTeleType[teleType] = telegramF;
            }
            catch (Exception eE)
            {
               LogManager.GetSingleton().ZLog("CD335", ELF.ERROR, "TelegramManagerTIA Start: Datenclient mit ID = {0} -> {1}", datenClientID, eE.Message);
            }

            try
            {
               teleType = "M";
               nameOfSubscriptionItem = "";
               itemNameOfSubscriptionItem = "";

               subscriptionItems = GetTelegramItems(teleType, TelegramConnectType.subscription);
               registrationItems = GetTelegramItems(teleType, TelegramConnectType.registration);


               TelegramM telegramM = (TelegramM)Telegram.NewInstance(itemNameOfSubscriptionItem, registrationItems, teleType);

               telegramsByTeleType[teleType] = telegramM;
            }
            catch (Exception eE)
            {
               LogManager.GetSingleton().ZLog("CD33C", ELF.ERROR, "TelegramManagerTIA Start: Datenclient mit ID = {0} -> {1}", datenClientID, eE.Message);
            }



            try
            {
               //Telegramme die für die Aufträge benötigt werden
               SqlRealtimeSimpleQuery query = new SqlRealtimeSimpleQuery(database, string.Format("select * from DC_AufträgeTIA where Datenclient_ID = {0} and Funktionsnummer >= 11 and Funktionsnummer <= 13 and Aktiv = 1 order by AuftragsNummer", datenClientID));

               if (query.QueryResult != null)
               {
                  foreach (Dictionary<string, object> prm2 in query.QueryResult)
                  {
                     try
                     {
                        //"A"-Telegramme sind für ClientAufträge
                        teleType = "A";
                        nameOfSubscriptionItem = "";
                        itemNameOfSubscriptionItem = "";

                        long auftragsID = Tools.GetLong(prm2, "ID");
                        long auftragsNummer = Tools.GetLong(prm2, "AuftragsNummer");

                        teleType += string.Format("{0}", auftragsNummer);


                        subscriptionItems = new Dictionary<string, string>();
                        registrationItems = GetTelegramAItems(auftragsID);


                        TelegramA telegramA = (TelegramA)Telegram.NewInstance(itemNameOfSubscriptionItem, registrationItems, teleType);

                        telegramA.AuftragsID = auftragsID;
                        telegramA.Auftragsnummer = auftragsNummer;

                        telegramsByTeleType[teleType] = telegramA;
                     }
                     catch (Exception e)
                     {
                        LogManager.GetSingleton().ZLog("C0266", ELF.ERROR, "Error TelegramManagerTIA.Start() {0}-> DatenClientID = {1}", e.Message, datenClientID);
                        return false;
                     }
                  }
               }
            }
            catch (Exception eA)
            {
               LogManager.GetSingleton().ZLog("CD336", ELF.ERROR, "Error TelegramManagerTIA.Start() {0}-> DatenClientID = {1}", eA.Message, datenClientID);
            }

         }


         return true;
      }
   }
}