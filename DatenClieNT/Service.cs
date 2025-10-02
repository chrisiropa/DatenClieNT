using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Text;
using System.Windows.Forms;
using System.Threading;

namespace DatenClieNT
{
   partial class Service : ServiceBase
   {
      public Service()
      {
         InitializeComponent();
      }
      
      private Boolean started = true;
      

      protected override void OnStart(string[] args)
      {
         string applicationPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
         
         if(!TheDC.GetSingleton().Start())
         {
            started = false;
            Stop();          
         }
      }

      protected override void OnStop()
      {
         if(started)
         {
            TheDC.GetSingleton().Stop();
         }
         else
         {
            MessageBox.Show(string.Format("DatenClieNT konnte nicht gestartet werden.\nEntweder konnte die Datenbank nicht gefunden werden oder die Verbindungseinstellungen in der Registry sind falsch !\nSiehe: HKEY_LOCAL_MACHINE\\SOFTWARE\\{0}", ConfigManager.IropaDatenClieNTRegistryPath), "", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error, System.Windows.Forms.MessageBoxDefaultButton.Button1, System.Windows.Forms.MessageBoxOptions.ServiceNotification);
         }
      }

   }
}
