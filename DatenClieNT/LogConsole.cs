using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;


namespace DatenClieNT
{
   class LogConsole
   {
      public LogConsole()
      {
         try
         {
            Console.SetWindowSize(100, 70);
            Console.BackgroundColor = ConsoleColor.Blue;
            Console.Clear();
         }
         catch
         {
            //Wenn die APP als Dienst läuft schmiert er hier ab !!!
         }
         
      }
   
      public void Log(LogEintrag logEintrag)
      {
         try
         {
            ConsoleColor color = Console.ForegroundColor;            
               
            switch(logEintrag.LogFlags)
            {
               case ELF.STATUS:
                  if (ConfigManager.GetSingleton().GetParameter("LOG_STATUS_CONSOLE", "1") != "1") return;
                  break;
               case ELF.INFO:
                  if (ConfigManager.GetSingleton().GetParameter("LOG_INFO_CONSOLE", "1") != "1") return;
               break;
               case ELF.WARNING:
                  if (ConfigManager.GetSingleton().GetParameter("LOG_WARNING_CONSOLE", "1") != "1") return;               
               break;
               case ELF.DEVELOPER:
                  if (ConfigManager.GetSingleton().GetParameter("LOG_DEVELOPER_CONSOLE", "1") != "1") return;
               break;
               case ELF.ERROR:
                  if (ConfigManager.GetSingleton().GetParameter("LOG_ERROR_CONSOLE", "1") != "1") return;
               break;
               case ELF.TELE:
                  if (ConfigManager.GetSingleton().GetParameter("LOG_TELE_CONSOLE", "1") != "1") return;
               break;
            }

            Console.ForegroundColor = logEintrag.GetColor(logEintrag.LogFlags);

            Console.WriteLine(string.Format("{4}:{2} {0} {3} {1}", logEintrag.ZeitStempel.ToString("dd.MM.yy HH:mm:ss.fff "), logEintrag.Text, logEintrag.LogFlagAsText, logEintrag.ThreadInfo, logEintrag.Identifier));
      
            Console.ForegroundColor = color;
         }
         catch
         {
         }         
      }
   }
}
