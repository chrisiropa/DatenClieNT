using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Reflection;
using System.Data.SqlClient;
using System.Data;
using System.Diagnostics;

namespace DatenClieNT
{
   public class TheDC
   {
      private Dictionary<long, DatenClient> datenClients = new Dictionary<long,DatenClient>();
      private AssemblyInfoWrapper assemblyInfo = new AssemblyInfoWrapper();      
      private static TheDC theDatenClientNET = null;
      private TimerManager globalTimerManager;
      private PerformanceCounter pcProcess;
      private SqlServerConnectionProvider sqlServerConnectionProvider = new SqlServerConnectionProvider();
      public static int DatabaseCriticalDelay = 500; //Später wieder auf 500 setzen
      
      public AssemblyInfoWrapper AssemblyInfoWrapper
      {
         get { return assemblyInfo; }
      }
      
      private TheDC()
      {
         pcProcess = new PerformanceCounter("Process", "% Processor Time", Process.GetCurrentProcess().ProcessName);
         globalTimerManager = TimerManager.NewInstance(GlobalEvent, "GLOBAL_TIMER");
      }
      

      public DatabaseConnectionProvider GetDatabaseConnectionProvider(string database)
      {
         return sqlServerConnectionProvider.GetDatabaseConnectionProvider(database);
      }

      
      public static TheDC GetSingleton()
      {
         if (theDatenClientNET == null)
         {
            theDatenClientNET = new TheDC();
         }

         return theDatenClientNET;
      }
      
      private void GlobalEvent(Evt evt)
      {
         if (evt.EvtType == Evt.Type.Timer)
         {
            EvtTimer timerEvt = (EvtTimer)evt;
            timerEvt.TimerEvent(timerEvt.Tag);
         }
      }

      private void GlobalTimerRefreshConfig(object tag)
      {
         ConfigManager.GetSingleton().Refresh();
      }
      
      public Boolean Start()
      {
         LogManager.GetSingleton().Init();
         
         ConfigManager.Init();

			
         globalTimerManager.Start();


         //Achtung, dieses ABO kann nicht so einfach gegen AddSingle ausgetauscht werden, da das
         //Wiederaufziehen aus dem gleichen Thread heraus passieren würde. Dadurch wird die 
         //Auflistung im TimerThread trotz "lock" geändert !
         globalTimerManager.AddAbo(GlobalTimerRefreshConfig,"RefreshConfig", 10000, false);


         LogManager.GetSingleton().Start(ConfigManager.GetSingleton().MainConnectionString);



         while (TheDC.GetSingleton().GetDatabaseConnectionProvider(ConfigManager.Database).ForceDatabaseConnection() == null)
         {
            LogManager.GetSingleton().ZLog("C0270", ELF.WARNING, "Datenbank noch nicht erreichbar... -> {0}", ConfigManager.GetSingleton().MainConnectionString);
            Thread.Sleep(ConfigManager.GetSingleton().TryDatabaseDelay);
         }

         
         
         
         LogManager.GetSingleton().ZLog("C0271", ELF.INFO, "------------------------------------------");
         LogManager.GetSingleton().ZLog("C0272", ELF.INFO, "Name      : {0} {1}", assemblyInfo.CompanyName, assemblyInfo.Name);
         LogManager.GetSingleton().ZLog("C0273", ELF.INFO, "Erstellt  : {0}", assemblyInfo.FileCreationTime);
         LogManager.GetSingleton().ZLog("C0274", ELF.INFO, "Modified  : {0}", assemblyInfo.FileModifiedTime);
         LogManager.GetSingleton().ZLog("C0275", ELF.INFO, "Version   : {0}.{1}.{2}.{3}", assemblyInfo.Major, assemblyInfo.Minor, assemblyInfo.Build, assemblyInfo.Revision);
         LogManager.GetSingleton().ZLog("C0276", ELF.INFO, "Datei     : {0}", assemblyInfo.ExecutionPath);
         LogManager.GetSingleton().ZLog("C0277", ELF.INFO, "Server    : {0}", ConfigManager.Server);
         LogManager.GetSingleton().ZLog("C0278", ELF.INFO, "Datenbank : {0}", ConfigManager.Database);
         LogManager.GetSingleton().ZLog("C0279", ELF.INFO, "------------------------------------------");
         
         LogManager.GetSingleton().LogsNachholen();

         LogManager.GetSingleton().ZLog("FFF0", ELF.INFO, "INFO-TEST");
         LogManager.GetSingleton().ZLog("FFF1", ELF.WARNING, "WARNING-TEST");
         LogManager.GetSingleton().ZLog("FFF2", ELF.ERROR, "ERROR-TEST");
         LogManager.GetSingleton().ZLog("FFF3", ELF.DEVELOPER, "DEVELOPER-TEST");
         LogManager.GetSingleton().ZLog("FFF4", ELF.TELE, "DEVELOPER-TEST");
         
         string dbConnectionInfo = string.Format("Server={0} DB={1} User={2}", ConfigManager.Server, ConfigManager.Database, ConfigManager.UserID);


         InitAndStartDatenclients(ConfigManager.Database);

      
         return true;
      }

      public bool Stop()
      {
         try
         {
            globalTimerManager.Stop();
            
            
            foreach (DatenClient datenClient in datenClients.Values)
            {
               datenClient.Stop();    
            }


            LogManager.GetSingleton().ZLog("C027A", ELF.INFO, "PROGRAMM BEENDET");
            
            LogManager.GetSingleton().Stop();
            
            Thread.Sleep(1000);
         }
         catch
         {
            System.Environment.Exit(0);
         }
         
         return true;
      }

      private bool Available(string database)
      {
         try
         {
            if (TheDC.GetSingleton().GetDatabaseConnectionProvider(database).ForceDatabaseConnection() == null)
            {
               LogManager.GetSingleton().ZLog("CD209", ELF.ERROR, "Datenbank {0} für externen DatenClient nicht erreichbar. Externer DatenClient wird ignoriert", database); 
               return false;              
            }
         }
         catch
         {
            LogManager.GetSingleton().ZLog("CD210", ELF.ERROR, "Datenbank {0} für externen DatenClient nicht erreichbar. Externer DatenClient wird ignoriert", database); 
            return false;
         }
      
         return true;
      }

      private void InitAndStartDatenclients(string mainDatabase)
      {
         LogManager.GetSingleton().ZLog("C027B", ELF.INFO, "Datenclients werden initialisiert");
         
         Dictionary<string, string> databaseNames = new Dictionary<string,string>();
         
         databaseNames[mainDatabase] = mainDatabase;

         try
         {
            SqlRealtimeSimpleQuery query = new SqlRealtimeSimpleQuery(mainDatabase, "select * from DC_DatenClientsExtern");
            
            if (query.QueryResult != null)
            {
               foreach (Dictionary<string, object> prm in query.QueryResult)
               {  
                  string database = "";
                  
                  try
                  {
                     database = Tools.GetString(prm, "DatabaseName");
                     
                     //Hier direkt mal aufmachen und Verfügbarkeit prüfen
                     //Mitloggen
                     if(Available(database))
                     {
                        databaseNames[database] = database;
                     }
                     else
                     {
                        LogManager.GetSingleton().ZLog("CD06F", ELF.ERROR, "TheDC Datenbank für externe DatenClieNTs nicht erreichbar !, DCs in Datenbank {0} werden ignoriert ", database);                        
                     }                     
                  }
                  catch
                  {
                  }
               }
            }
         }
         catch
         {            
         }
         
         

         Boolean somethingFound = false;
         
         //Wenn Spalte "Deaktiviert" = True ist wird der DatenClient gar nicht geladen und kann auch nicht
         //nachträglich aktiviert werden.
         //Wenn die Spalte "Aktiv" = False ist und "Deaktiviert" = False, dann wird der Datenclient geladen
         //und kann jederzeit online aktiv und auch wieder inaktiv geschaltet werden. 
         //Man braucht dazu nicht das Program neu starten.
         
         foreach(string database in databaseNames.Keys)
         {
            //Dies ist ein reiner TIA DatenClient.....
            //Also nur die TIA DC's aus der Tabelle rausholen...
            SqlRealtimeSimpleQuery query = new SqlRealtimeSimpleQuery(database, "select * from DC_DatenclientsTIA where Deaktiviert = 0 order by Anlagen_ID");

            if (query.QueryResult != null)
            {
               foreach (Dictionary<string, object> prm in query.QueryResult)
               {  
                  string bezeichnung = "";
                  
                  try
                  {
                     SystemType systemType = SystemType.Unknown;
                     
                     long id = Tools.GetLong(prm, "ID");
                     bezeichnung = Tools.GetString(prm, "Bezeichnung");
                     
                     systemType = SystemType.TIA;

                     DatenClient datenClient = DatenClient.NewDatenClient(systemType);
                     
                     datenClient.PreInit(database);
                     
                     if(datenClient != null)
                     {
                        datenClients[id] = datenClient;                           
                        somethingFound = true;
                     }
                     else
                     {
                        LogManager.GetSingleton().ZLog("C027C", ELF.ERROR, "Datenclient {0} konnte nicht erzeugt werden.", bezeichnung);
                     }
                  }
                  catch (Exception e)
                  {
                     LogManager.GetSingleton().ZLog("C027D", ELF.ERROR, "Error in TheDatenClientNET.InitAndStartDatenclients() {1}-> {0}", e.Message, bezeichnung);
                  }
               }
            }
         }

         try
         {
            foreach (long id in datenClients.Keys)
            {
               if (!datenClients[id].Init(id))
               {
                  LogManager.GetSingleton().ZLog("C027F", ELF.ERROR, "Konfigurationsfehler !Datenclient {0} konnte nicht gestartet werden", datenClients[id].Name);
                  continue;
               }
               if (!datenClients[id].Start())
               {
                  LogManager.GetSingleton().ZLog("C0280", ELF.ERROR, "Datenclient {0} konnte nicht gestartet werden", datenClients[id].Name);
               }
            }
         }
         catch (Exception e)
         {
            LogManager.GetSingleton().ZLog("C0281", ELF.ERROR, "Datenclient.Init/Start -> {0} ", e.Message);
         }
         
         if(!somethingFound)
         {
            LogManager.GetSingleton().ZLog("C0282", ELF.WARNING, "Keine (oder keine gültigen) Datenclients gefunden");
         }
      }
   }
}

