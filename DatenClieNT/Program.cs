using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using System.Resources;
using System.Reflection;
using System.Threading;
using System.ServiceProcess;
using System.Configuration;
using System.Runtime.InteropServices;
using Microsoft.VisualBasic;
using Microsoft.Win32;


namespace DatenClieNT
{
   class Program
   {
      private static bool runAsService = true;
      private static bool installServiceMode = false;
      public static Boolean console = false;
      public static Boolean useInatWrapper = false;

      private static string applicationName = System.Diagnostics.Process.GetCurrentProcess().ProcessName.Replace(".vshost", "");
      private static string applicationPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
      private static string applicationTitle = "IROPA DatenClieNT_TIA";
      private static string applicationDescription = "Schnittstelle SPS/Datenbank";

      public static Boolean IsService
      {
         get { return (runAsService == true); }
      }
      
      public static string ApplicationDescription
      {
         get { return applicationDescription; }
      }

      public static string ApplicationTitle
      {
         get { return applicationTitle; }
      }

      
      

      private static void CONSOLE()
      {

         //object value = System.Single.MaxValue;

         if (TheDC.GetSingleton().Start())
         {         
            Console.ReadLine();         
            TheDC.GetSingleton().Stop();
         }
      }
      
      public static void Main(string[] args)
      {
         LogManager.InitEmergencyLog();
         LogManager.EmergencyLog("Programmstart: {0}", DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss.fff"));
      
         Boolean aufrufVomInnoSetup = false;

         console = false;

         foreach (string arg in args)
         {
            if (arg.Contains("SetupCall"))
            {
               aufrufVomInnoSetup = true;

               LogManager.EmergencyLog("Setup Call: {0}", DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss.fff"));
               
               break;
            }
         }

         LogManager.EmergencyLog("Initiale Position 1: {0}", DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss.fff"));
         
         foreach(string arg in args)
         {
            //Beim Debuggen ist als Befehlszeilenargument "CONSOLE" eingestellt.
            //Daher wird also beim Arbeiten mit dem DeveloperStudio der DC immer
            //als Konsole gestartet !!!!!!!!!!
            if(arg.ToUpper().Contains("CONSOLE"))
            {
               LogManager.EmergencyLog("Initiale Position CONSOLE Parameter gefunden: {0}", DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss.fff"));
               
               break;
            }
         }
      
         try
         {      
            //useInatWrapper auf true oder false setzen
            RegistryKey iropaDatenclientKey = Registry.LocalMachine.OpenSubKey(ConfigManager.IropaDatenClieNTRegistryPath);

            if (iropaDatenclientKey == null)
            {
               iropaDatenclientKey = Registry.LocalMachine.CreateSubKey(ConfigManager.IropaDatenClieNTRegistryPath);
               
               if(iropaDatenclientKey != null)
               {
                  iropaDatenclientKey.SetValue("InatAutoSwitchServiceApplication", "0");
               }
            }

            if (iropaDatenclientKey != null)
            {
               if (((string)iropaDatenclientKey.GetValue("InatAutoSwitchServiceApplication")) == null)
               {
                  Registry.LocalMachine.OpenSubKey(ConfigManager.IropaDatenClieNTRegistryPath, true).SetValue("InatAutoSwitchServiceApplication", "0");
               }
            
               if(((string)iropaDatenclientKey.GetValue("InatAutoSwitchServiceApplication")) == "0")
               {
                  useInatWrapper = false;
               }
               else
               {
                  useInatWrapper = true;
               }
            }
         }
         catch
         {
         }
         
         
         
         try
         {
            if(args[0].ToUpper().Contains("CONSOLE"))
            {
               console = true;
            }
         }
         catch
         {         
         }

         
         if (console)
         {
            CONSOLE();
            return;
         }
         else
         {
            //INAT als Dienst installieren
         }

         if (AccountInfo.IsService())
         {
            //Als Dienst gestartet
            installServiceMode = false;
            runAsService = true;
         }
         else
         {
            //Von Console gestartet (Dienst installieren)
            installServiceMode = true;
            runAsService = false;
         }


         if (installServiceMode)
         {
            if (!ServiceExists())
            {
               InstallService();
               Console.WriteLine(string.Format("Dienst {0} installiert !", applicationName));
               if (!aufrufVomInnoSetup)
               {
                  Console.WriteLine("Enter drücken zum Beenden !");
                  Console.ReadLine();
               }
            }
            else
            {
               if (ServiceRunning())
               {
                  if (!aufrufVomInnoSetup)
                  {
                     Console.WriteLine(string.Format("Dienst {0} läuft gerade ! !", applicationName));
                     Console.WriteLine(string.Format("Erst beenden !"));
                  }
                  else
                  {
                     Console.WriteLine(string.Format("Dienst {0} läuft gerade ! !", applicationName));
                  }
               }
               else
               {
                  if(!aufrufVomInnoSetup)
                  {
                     UnInstallService();
                     Console.WriteLine(string.Format("Dienst {0} deinstalliert !", applicationName));
                     Console.WriteLine("Enter drücken zum Beenden !");
                     Console.ReadLine();
                  }
                  else
                  {
                     Console.WriteLine(string.Format("Dienst {0} ist bereits installiert !", applicationName));
                     Thread.Sleep(1500);
                  }
               }
            }
         }
         else
         {
            if (runAsService)
            {
               if (ServiceExists())
               {  
                  ServiceBase[] services = new ServiceBase[] { new Service() };
                  ServiceBase.Run(services);
               }
            }
         }
      }

      static bool ServiceExists()
      {
         bool installed = false;

         ServiceController[] controllers = ServiceController.GetServices();
         foreach (ServiceController con in controllers)
         {
            if (con.ServiceName == applicationName)
            {
               installed = true;
               break;
            }
         }

         return installed;
      }

      static bool ServiceRunning()
      {
         bool running = false;

         ServiceController[] controllers = ServiceController.GetServices();
         foreach (ServiceController con in controllers)
         {
            if (con.ServiceName == applicationName)
            {
               if (con.Status == ServiceControllerStatus.Running)
               {
                  running = true;
               }
               break;
            }
         }

         return running;
      }

      static void InstallService()
      {
         ServiceInstaller.InstallService("\"" + applicationPath + "\\" + applicationName + ".exe\" -service", applicationName, applicationDescription, applicationTitle, true, false);
      }

      static void UnInstallService()
      {
         ServiceController controller = new ServiceController(applicationName);
         if (controller.Status == ServiceControllerStatus.Running)
         {
            controller.Stop();
         }
         if (!ServiceInstaller.UnInstallService(applicationName))
         {
            Console.WriteLine("FEHLER beim Deinstallieren....");
         }
      }
   }
}
