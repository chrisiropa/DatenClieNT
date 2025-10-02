using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace DatenClieNT
{
   class LogFile
   {
      private const long maxLogfileEntries = 200000; //Entspricht im Mittel 20MB Filegröße
      private long logfileEntryCounter = 0;
      private bool firstError = true;
      
      public LogFile()
      {
         SetCurrentFileToHistoryFile();
         
         
         try
         {
            FileStream fileStream = new FileStream(ConfigManager.LogfilePath, FileMode.Append, FileAccess.Write, FileShare.Write);
            fileStream.Close();
         }
         catch (Exception e)
         {
            Console.WriteLine("Exception in LogFile.LogFile -> {0}", e.Message);            
         }
      }

      private void InitEntryCounter()
      {
         string thePath = ConfigManager.LogfilePath;

         if (File.Exists(thePath))
         {
            StreamReader reader = new StreamReader(thePath);

            logfileEntryCounter = 0;
            while (!reader.EndOfStream)
            {
               reader.ReadLine();
               logfileEntryCounter++;
            }
            reader.Close();
         }
         else
         {
            logfileEntryCounter = 0;
         }
      }


      private bool SetCurrentFileToHistoryFile()
      {
         string thePath = ConfigManager.LogfilePath;
         
         bool success = true;
         logfileEntryCounter = 0;

         try
         {
            if (File.Exists(thePath))
            {
               string historyFile = thePath.Insert(thePath.Length - 4, DateTime.Now.ToString(" ddMMyyyyHHmmssfff"));

               FileInfo fi = new FileInfo(thePath);
               fi.MoveTo(historyFile);
            }
         }
         catch (Exception e4)
         {
            success = false;

            Console.WriteLine("Exception in LogFile.SetCurrentFileToHistoryFile -> {0}", e4.Message);  
            
         }

         return success;
      }

      public void Log(LogEintrag le)
      {
         switch (le.LogFlags)
         {
            case ELF.STATUS:
               if (ConfigManager.GetSingleton().GetParameter("LOG_STATUS_FILE", "1") != "1") return;
               break;
            case ELF.INFO:
               if (ConfigManager.GetSingleton().GetParameter("LOG_INFO_FILE", "1") != "1") return;
               break;
            case ELF.WARNING:
               if (ConfigManager.GetSingleton().GetParameter("LOG_WARNING_FILE", "1") != "1") return;
               break;
            case ELF.DEVELOPER:
               if (ConfigManager.GetSingleton().GetParameter("LOG_DEVELOPER_FILE", "1") != "1") return;
               break;
            case ELF.ERROR:
               if (ConfigManager.GetSingleton().GetParameter("LOG_ERROR_FILE", "1") != "1") return;
               break;
            case ELF.TELE:
               if (ConfigManager.GetSingleton().GetParameter("LOG_TELE_FILE", "1") != "1") return;
               break;
         }

         string text = string.Format("{0}{1}{5}{2}{5}{3}{5}{7}{6}: {4}", Ascii.SOH, le.LogFlagAsText, le.ZeitStempel.ToString("dd.MM.yy HH:mm:ss.fff"), le.Identifier, le.Text, "'", Ascii.STX, le.ThreadInfo);

         
         
         bool written = false;
         long tryCounter = 0;

         string thePath = ConfigManager.LogfilePath;

         while ((!written) && (tryCounter < 10))
         {
            try
            {
               StreamWriter streamWriter = new StreamWriter(thePath, true, Encoding.UTF8);
               streamWriter.WriteLine(text);
               streamWriter.Close();
               written = true;
               logfileEntryCounter++;
               
               if((logfileEntryCounter % 100) == 0)
               {
                  DeleteOldLogFiles();
               }

               if (logfileEntryCounter > maxLogfileEntries)
               {
                  if (!SetCurrentFileToHistoryFile())
                  {
                     logfileEntryCounter = 0;
                     streamWriter = new StreamWriter(thePath, true, Encoding.UTF8);
                     streamWriter.WriteLine(text);
                     streamWriter.Close();
                  }
               }
            }
            catch (Exception e5)
            {
               if(firstError)
               {
                  //Nicht die Konsole vollpflastern, wenn LogFile-Pfad in der Datenbank falsch 
                  //konfiguriert ist.
                  firstError = false;
                  Console.WriteLine("Exception in LogFile.Log -> {0}", e5.Message);
                  Console.WriteLine("   Ist der Pfad in der Datenbank korrekt eingestellt ?"); 
               }
               
               tryCounter++;
               System.Threading.Thread.Sleep(0);
            }
         }
      }

      private void DeleteOldLogFiles()
      {
         //[ ] Alle 100 Einträge das älteste DatenClieNT-Logfile raussuchen 
         //[ ] Testen ob es älter ist als 3 Monate 
         //[ ] Wenn ja löschen    

         DirectoryInfo parent = new DirectoryInfo(Path.GetDirectoryName(ConfigManager.LogfilePath));

         List<FileInfo> children = new List<FileInfo>();

         foreach (FileInfo child in parent.GetFiles())
         {
            if (child.FullName.Contains(Path.GetFileNameWithoutExtension(ConfigManager.LogfilePath)))
            {
               children.Add(child);
            }
         }
         
         if (children.Count == 0)
         {
            return;
         }

         FileInfo oldest = children[0];

         foreach (FileInfo nextFile in children)
         {
            if (nextFile.LastWriteTimeUtc < oldest.LastWriteTimeUtc)
            {
               oldest = nextFile;               
            }
         }  
         
         
         TimeSpan timeSpan = DateTime.UtcNow - oldest.LastWriteTimeUtc;
         
         string logFileKeepDays = ConfigManager.GetSingleton().GetParameter("LOGFILE_KEEP_DAYS", string.Format("{0}", ConfigManager.MaxFileKeepDays));
         int keepDays = Convert.ToInt32(logFileKeepDays);


         if (timeSpan.TotalDays > keepDays)
         {
            //Nur löschen wenn es nicht die Original DCNET.LOG ist !!!!!!!!!!!!!!!
            if (oldest.FullName.ToUpper() != ConfigManager.LogfilePath.ToUpper())
            {
               try
               {
                  File.Delete(oldest.FullName);
               }
               catch
               {
               }
            }
         }
             
      }    
   }
}
