using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Configuration;
using System.Data.SqlClient;
using System.Reflection;
using System.IO;
using Microsoft.Win32;
using System.Linq;

namespace DatenClieNT
{

   public class ConfigManager
   {
      private Boolean firstRefresh = true;
      private Int64 timestamp = 0;
      private string mainConnectionString;
      private Dictionary<string, string> specificConnectionStrings = new Dictionary<string, string>();
      private string executionDirectory;
		private int licenseFailedCounter = 0;
      private int maxLicenseFailed = 3;
      
      public static int FirstTimeout = 4500;
      public static int SecondTimeout = 4000;
      
      public static int MaxFileKeepDays = 120;
      private int tryDatabaseDelay = 4000;

		public Boolean programmNummer_alt = false;

      public static string IropaDatenClieNTRegistryPath = "SOFTWARE\\IROPA\\DatenClieNT_TIA";
      private static string logfileName = "DC_TIA.LOG";

      private static ConfigManager config = null;

      private Dictionary<string, string> parameters = new Dictionary<string, string>();

      public static string KeyInatStatusAus = "InatStatusAus";
      public static string KeyServer = "Server";
      public static string KeyDatabase = "Database";
      public static string KeyUserID = "UserID";
      public static string KeyPassword = "Password";
      public static string KeyLanguage = "Language";
      public static string KeyConnectTimeout = "ConnectTimeout";
      public static string KeyDiagnosePort = "DiagnosePort";
      public static string KeyLogfilePath = "LogfilePath";
      public static string KeyUpdateStatusTimeout = "UpdateStatusTimeout";

      private static string defaultDatabase = "Z.B. FA3524";
      private static string defaultServer = "localhost";
      private static string defaultUserID = "iropa";
      private static string defaultPassword = "sa";
      private static string defaultLanguage = "German";
      private static string defaultConnectTimeout = "10";
      private static string defaultDiagnosePort = "5001";
      private static string defaultUpdateStatusTimeout = "1000";

      public static string InatStatusAus;
      public static string Server;
      public static string Database;
      public static string UserID;
      public static string Password;
      public static string Language;
      public static string ConnectTimeout;
      public static string DiagnosePort;
      public static string LogfilePath;
      private static string updateStatusTimeout;



      public string MainConnectionString
      {
         get { return mainConnectionString; }
      }

      public int TryDatabaseDelay
      {
         get { return tryDatabaseDelay; }
      }
      
      public static int TimeoutAsSeconds(int timeoutMS)
      {
         int timeoutSeconds = timeoutMS / 1000;
         
         if(timeoutSeconds < 1)
         {
            timeoutSeconds = 1;
         }
         
         return timeoutSeconds;
      }

      private ConfigManager()
      {
         try
         {

            AssemblyInfoWrapper iw = new AssemblyInfoWrapper();
            executionDirectory = Path.GetDirectoryName(iw.ExecutionPath);

            ConfigManager.InatStatusAus = GetSetValue(KeyInatStatusAus, "0");
            ConfigManager.Server = GetSetValue(KeyServer, defaultServer);
            ConfigManager.Database = GetSetValue(KeyDatabase, defaultDatabase);
            ConfigManager.UserID = GetSetValue(KeyUserID, defaultUserID);
            ConfigManager.Password = GetSetValue(KeyPassword, defaultPassword);
            ConfigManager.Language = GetSetValue(KeyLanguage, defaultLanguage);
            ConfigManager.ConnectTimeout = GetSetValue(KeyConnectTimeout, defaultConnectTimeout);
            ConfigManager.DiagnosePort = GetSetValue(KeyDiagnosePort, defaultDiagnosePort);
            ConfigManager.LogfilePath = string.Format("{0}\\{1}", GetSetValue(KeyLogfilePath, executionDirectory), logfileName);
            ConfigManager.updateStatusTimeout = GetSetValue(KeyUpdateStatusTimeout, defaultUpdateStatusTimeout);
            SqlConnectionStringBuilder csb = new SqlConnectionStringBuilder();

            csb.DataSource = Server;
            csb.InitialCatalog = Database;
            try
            {
               csb.ConnectTimeout = Convert.ToInt32(ConnectTimeout);
            }
            catch
            {
               csb.ConnectTimeout = 30;
            }
            csb.CurrentLanguage = Language;
            csb.UserID = UserID;
            csb.Password = Password;
            csb.ApplicationName = Program.ApplicationTitle;
            csb.Pooling = true;

            //ConnectionString            
            mainConnectionString = csb.ConnectionString;

            Console.WriteLine("Main-ConnectionString = {0}", mainConnectionString);           
            

         }
         catch (Exception e)
         {
            Console.WriteLine("ConfigManger.ConfigManager{0}", e.Message);
         }
      }

      public string SpecificConnectionString(string database)
      {
         if (!specificConnectionStrings.ContainsKey(database))
         {
            SqlConnectionStringBuilder csb = new SqlConnectionStringBuilder();

            csb.DataSource = ConfigManager.Server;
            csb.InitialCatalog = database;
            try
            {
               csb.ConnectTimeout = Convert.ToInt32(ConfigManager.ConnectTimeout);
            }
            catch
            {
               csb.ConnectTimeout = 30;
            }
            csb.CurrentLanguage = ConfigManager.Language;
            csb.UserID = ConfigManager.UserID;
            csb.Password = ConfigManager.Password;
            csb.ApplicationName = Program.ApplicationTitle;
            csb.Pooling = true;
            csb.AsynchronousProcessing = true;

            //ConnectionString            
            specificConnectionStrings[database] = csb.ConnectionString;
         }

         return specificConnectionStrings[database];
      }

      private static string GetSetValue(string key, string defaultValue)
      {
         //Erstmal nur lesend zugreifen, falls dieses Programm kein Dienst ist bzw. nicht als Admin gestartet wurde (Debug oder als Console),
         //darf man unter WIN7 und Server2008 nicht schreiben
         RegistryKey iropaDatenclientKey = Registry.LocalMachine.OpenSubKey(IropaDatenClieNTRegistryPath, false);

         if (iropaDatenclientKey == null)
         {
            //Eintrag noch nicht da...
            //... also versuchen zu erzeugen
            try
            {
               iropaDatenclientKey = Registry.LocalMachine.CreateSubKey(IropaDatenClieNTRegistryPath);
               iropaDatenclientKey.SetValue(key, defaultValue);
               iropaDatenclientKey.Close();
               LogManager.GetSingleton().ZLog("C0057", ELF.INFO, "ConfigManager.GetSetValue: Registry-Eintrag {0} wurde soeben erzeugt. Wert = {1}", key, defaultValue);
               return defaultValue;
            }
            catch
            {
               //Falls es misslingt...
               //...ist dies wohl kein Dienst und das Programm wurde auch nicht als Admin ausgeführt
               //   daher hier abbrechen und den Standardwert zurückgeben
               LogManager.GetSingleton().ZLog("C0058", ELF.WARNING, "ConfigManager.GetSetValue POS0: Registry konnte nicht beschrieben werden. Programm als Admin oder Service starten !");
               return defaultValue;
            }
         }

         string value = (string)iropaDatenclientKey.GetValue(key);
         if (value == null)
         {
            value = defaultValue;

            try
            {
               //Registry-Zweig ist zwar da, aber der Key existriert noch nicht.
               //Daher erstmal lesenden Zweig zumachen und beschreibbar öffnen
               //und defaultWert eintragen
               iropaDatenclientKey.Close();
               iropaDatenclientKey = Registry.LocalMachine.OpenSubKey(IropaDatenClieNTRegistryPath, true);
               iropaDatenclientKey.SetValue(key, value);
               iropaDatenclientKey.Close();
               return value;
            }
            catch
            {
               LogManager.GetSingleton().ZLog("C0059", ELF.WARNING, "ConfigManager.GetSetValue POS1 : Key konnte nicht beschrieben werden. Programm als Admin oder Service starten !");
            }
         }

         return value;
      }

      public static ConfigManager GetSingleton()
      {
         return config;
      }

      public static void Init()
      {
         if (config == null)
         {
            config = new ConfigManager();
         }
      }

      public string GetParameter(string name, string standardWert)
      {
         string value = standardWert;

         lock (parameters)
         {
            if (parameters.ContainsKey(name))
            {
               value = parameters[name];
            }
         }

         return value;
      }

      public Boolean RefreshAlarmTrimSpaces()
      {
         using (SqlConnection sqlConnection = TheDC.GetSingleton().GetDatabaseConnectionProvider(ConfigManager.Database).GetLowPrioOpenDBConnection())
         {
            if (sqlConnection == null)
            {
               return false;
            }
         }
        
         try
         {  
            SqlRealtimeSimpleQuery query = new SqlRealtimeSimpleQuery(Database, "select * from DC_Parameter where Name = 'ALARM_TRIM_SPACES' or Name = 'Programmnummer_ALT'");

            lock (parameters)
            {
               if (query.QueryResult != null)
               {
                  foreach (Dictionary<string, object> prm in query.QueryResult)
                  {
                     try
                     {
                        parameters[(string)prm["Name"]] = (string)prm["Wert"];
                     }
                     catch (Exception e0)
                     {
                        LogManager.GetSingleton().ZLog("C005D", ELF.ERROR, "Error in Parameter.Refresh() -> {0}", e0.Message);
                     }
                  }


                  LogManager.GetSingleton().ZLog("C005E", ELF.INFO, "PARAMETER START ----------------------------------------");

                  foreach (string key in parameters.Keys)
                  {
                     LogManager.GetSingleton().ZLog("C005F", ELF.INFO, "   {0}={1}", key, parameters[key]);
                  }

                  LogManager.GetSingleton().ZLog("C0060", ELF.INFO, "PARAMETER ENDE -----------------------------------------");
               }
            }
         }
         catch
         {
            return false;
         }
         
         return true;
      }

      public void Refresh()
      {
         HiresStopUhr stopUhr = new HiresStopUhr();
         stopUhr.Start();

         using (SqlConnection sqlConnection = TheDC.GetSingleton().GetDatabaseConnectionProvider(ConfigManager.Database).GetLowPrioOpenDBConnection())
         {
            if (sqlConnection == null)
            {
               return;
            }
         }

         long connectDelay = stopUhr.IntermediateMilliSeconds;


         Int64 ts = 0;
         string statement = "select Convert(bigint, max(TS)) as TS from DC_Parameter";
         
         try
         {

            SqlRealtimeSimpleQuery tsQuery = new SqlRealtimeSimpleQuery(100, ConfigManager.Database, statement);

            if (tsQuery.QueryResult != null)
            {
               Dictionary<string, object> obj = tsQuery.QueryResult[0];

               ts = Convert.ToInt64(obj["TS"]);

            }
         }
         catch (Exception e)
         {
            if (firstRefresh)
            {
               //throw new Exception(string.Format("Der erste Zugriff auf die Datenbank ist fehlgeschlagen !\nIst in der Registry unter {0} alles richtig eingestellt ?\nDer generierte ConnectionString lautet:\n{1}", IropaDatenClieNTRegistryPath, mainConnectionString));
            }
            
            //Refresh nicht so wichtig.
            //NICHT neu aufsetzen deswegen
            LogManager.GetSingleton().ZLog("C005B", ELF.WARNING, "ConfigManager.Refresh(0) -> {0}", e.Message);
            return;
         }

         firstRefresh = false;

         stopUhr.Stop();

         if (stopUhr.PeriodMilliSeconds > TheDC.DatabaseCriticalDelay)
         {
            LogManager.GetSingleton().ZLog("CD101", ELF.WARNING, "DB-Zugriff dauerte länger als {0}ms -> Statement=select Convert(bigint, max(TS)) as TS from DC_Parameter | DauerExecute={1} davon Connect = {2}", TheDC.DatabaseCriticalDelay, stopUhr.PeriodMilliSeconds, connectDelay);
         }

         LogManager.GetSingleton().ZLog("CD200", ELF.DEVELOPER, "DB-DELAY {0}ms {1}", stopUhr.PeriodMilliSeconds, statement);


         if (ts > timestamp)
         {
            if (timestamp != 0)
            {
               LogManager.GetSingleton().ZLog("C005C", ELF.INFO, "Parameter in der Datenbank haben sich geändert. Sie werden jetzt neu einglesen.");
            }

            timestamp = ts;

            try
            {

               stopUhr = new HiresStopUhr();
               stopUhr.Start();

               SqlRealtimeSimpleQuery query = new SqlRealtimeSimpleQuery(Database, "select * from DC_Parameter where not Name = 'ALARM_TRIM_SPACES'");

               lock (parameters)
               {
                  parameters.Clear();

                  if (query.QueryResult != null)
                  {
                     foreach (Dictionary<string, object> prm in query.QueryResult)
                     {
                        try
                        {
                           parameters[(string)prm["Name"]] = (string)prm["Wert"];
                        }
                        catch (Exception e0)
                        {
                           LogManager.GetSingleton().ZLog("CD25B", ELF.ERROR, "Error in Parameter.Refresh() -> {0}", e0.Message);
                        }
                     }
                  }

                  
               }

               stopUhr.Stop();

               if (stopUhr.PeriodMilliSeconds > TheDC.DatabaseCriticalDelay)
               {
                  LogManager.GetSingleton().ZLog("CD102", ELF.WARNING, "DB-Zugriff dauerte länger als {0}ms -> Statement=select * from DC_Parameter | DauerExecute={1}", TheDC.DatabaseCriticalDelay, stopUhr.PeriodMilliSeconds);
               }
            }
            catch (Exception e)
            {
               LogManager.GetSingleton().ZLog("C0061", ELF.ERROR, "ConfigManager.Refresh (1) konnte nicht ausgeführt werden. Möglicherweise ist die Datenbank nicht mehr erreichbar. Es wird mit der bestehenden Konfiguration weitergemacht. -> {0}", e.Message);
               //Refresh nicht so wichtig.
               //NICHT neu aufsetzen deswegen
            }
         }

         VerifyLicense vm = new VerifyLicense();
         if(!vm.Check("MAC") || !vm.Check("EXPIRE"))
         {
            //Programm wurde manipuliert !
            licenseFailedCounter++;

            LogManager.GetSingleton().ZLog("C0270", ELF.ERROR, "DatenClieNT-Programm MAC Adresse nicht lizensiert oder Datum abgelaufen ! Zum {0}. Mal festgestellt", licenseFailedCounter);
            LogManager.GetSingleton().ZLog("C0270", ELF.ERROR, "Beim {0}. Mal wird das Programm beendet.", maxLicenseFailed);

            if(licenseFailedCounter >= maxLicenseFailed) 
            {
               System.Environment.Exit(0);
            }
         }
      }

      public long GetParameterAsLong(string name, long standardWert)
      {
         long parameter = standardWert;

         try
         {
            parameter = Convert.ToInt64(GetParameter(name, standardWert.ToString()));
         }
         catch (Exception e)
         {
            LogManager.GetSingleton().ZLog("C0062", ELF.ERROR, "ConfigManager.GetParameterAsLong -> {0},{1}, -> {2}", name, standardWert, e.Message);
         }

         return parameter;
      }
   }
}
