using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading;
using System.Reflection;
using System.Diagnostics;



namespace DatenClieNT
{
   public class LogManager
   {
      private LogZentrale logZentrale; 
      private LogFile logFile;
      private LogConsole logConsole;
      private static LogManager logging = null;
      private bool running = false;
      private bool stopped = false;
      private static string emergencyLogPath = "";

      private List<LogEintrag> initPhaseLogBuffer = new List<LogEintrag>();

		
      private LogManager()
      {         
      }
      
      public static void LowLevelLog(string filename, string text)
      {
         try
         {
            StreamWriter streamWriter = new StreamWriter(filename, true, Encoding.UTF8);
            streamWriter.WriteLine(text);
            streamWriter.Close();
         }
         catch
         {
            System.Threading.Thread.Sleep(0);
         }
      }

      public void ZLog(string identifier, ELF logFlags, string formatString, params object[] paramObjects)
      {
         if(stopped)
         {
            return;
         }

         LogEintrag logEintrag;

         DateTime logTime = DateTime.Now;
                  
         string threadInfo = "";
			int threadID = Thread.CurrentThread.ManagedThreadId;
			
			lock(DatenClient.Threads)
			{
				if(DatenClient.Threads.ContainsKey(threadID))
				{
					threadInfo = string.Format("{0}({1})", DatenClient.Threads[threadID], threadID);
				}
				else
				{
					threadInfo = string.Format("{0}|{1}", Thread.CurrentThread.Name, threadID);
				}
			}
			

         try
         {
            try
            {
               if (ConfigManager.GetSingleton().GetParameter("LOG_DATETIME_AS_UTC", "0") == "1")
               {
                  logTime = DateTime.UtcNow;
               }

               logEintrag = new LogEintrag(identifier, logFlags, string.Format(formatString, paramObjects), logTime, threadInfo);
            }
            catch
            {
               logEintrag = new LogEintrag(identifier, logFlags, string.Format("Fehlerhaft formatierter LogEintrag -> {0}", formatString), logTime, threadInfo);
            }


            if (running)
            {
               logging.Log(logEintrag);
            }
            else
            {
               initPhaseLogBuffer.Add(logEintrag);
            }
         }
         catch (Exception)
         {
            Console.WriteLine("LOGGING noch nicht aktiv");
         }
      }
      
      
      public static void InitEmergencyLog()
      {
         AssemblyInfoWrapper aiw = new AssemblyInfoWrapper();

         emergencyLogPath = string.Format("{0}\\Emergency.log", Path.GetDirectoryName(aiw.ExecutionPath));

         try
         {
            try
            {
               File.Delete(emergencyLogPath);
            }
            catch
            {
            }

            FileStream fileStream = new FileStream(emergencyLogPath, FileMode.Append, FileAccess.Write, FileShare.Write);
            fileStream.Close();
         }
         catch
         {
         }
      }

      public static void EmergencyLog(string formatString, params object[] paramObjects)
      {
         LogEintrag logEintrag;
         string text = "";
         
         string threadInfo = string.Format("{0}:{1}", System.Threading.Thread.CurrentThread.Name, System.Threading.Thread.CurrentThread.ManagedThreadId);

         try
         {
            logEintrag = new LogEintrag("", ELF.ERROR, string.Format(formatString, paramObjects), DateTime.Now, threadInfo);

            text = string.Format("{2} {0} {3} {1}", logEintrag.ZeitStempel.ToString("dd.MM.yy HH:mm:ss.fff "), logEintrag.Text, "EMERGENCY", logEintrag.ThreadInfo);
         }
         catch
         {
            return;
         }
         

         bool written = false;
         long tryCounter = 0;

         while ((!written) && (tryCounter < 10))
         {
            try
            {
               StreamWriter streamWriter = new StreamWriter(emergencyLogPath, true, Encoding.UTF8);
               streamWriter.WriteLine(text);
               streamWriter.Close();
               written = true;               
            }
            catch
            {
               tryCounter++;
               System.Threading.Thread.Sleep(0);
            }
         }
      }

      public void LogsNachholen()
      {
         try
         {
            if (logging != null)
            {
               foreach (LogEintrag logEintrag in initPhaseLogBuffer)
               {
                  if (logging != null)
                  {
                     logging.Log(logEintrag);
                  }
               }
            }
            initPhaseLogBuffer = new List<LogEintrag>();
         }
         catch(Exception e)
         {
            Console.WriteLine("LogManager.LogsNachholen {0}", e.Message);
         }
      }
      
      private static object lockObject = new object();
      
      public static LogManager GetSingleton()
      {
         lock(lockObject)
         {         
            if(logging == null)
            {
               logging = new LogManager();
            }
         }
         
         return logging;
      }
      
      public void Init()
      {
         //Nicht löschen !
      }
      
      
      public void Start(string connectionString)
      {
         try
         {
            logZentrale = new LogZentrale();
            logZentrale.Start();

            logFile = new LogFile();
            logZentrale.Register(logFile.Log);
            
            logConsole = new LogConsole();
            logZentrale.Register(logConsole.Log);
            
            running = true; 
         }
         catch
         {
         }
      }

      public void Stop()
      {
         logZentrale.Stop();

         stopped = true; 
      }

      private void Log(LogEintrag logEintrag)
      {
         logZentrale.Log(logEintrag);         
      }
   }
}
