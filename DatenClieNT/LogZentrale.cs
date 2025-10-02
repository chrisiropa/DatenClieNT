using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.IO;

namespace DatenClieNT
{
   public delegate void WriteDelegate(LogEintrag logEintrag);
   
   public class LogZentrale
   {
      private Queue logEintraege;
      private event WriteDelegate writeEvent;
      private Thread dispatcherThread;
      private volatile bool terminate = false;
      
      
      public LogZentrale()
      {
         logEintraege = Queue.Synchronized(new Queue(50000));
      }

      public bool Start()
      {
         dispatcherThread = new Thread(new ThreadStart(DispatcherThreadFunc));
         dispatcherThread.Start();
         
         return true;
      }

      public bool Stop()
      {
         terminate = true;
         lock (logEintraege)
         {
            Monitor.PulseAll(logEintraege);
         }
         dispatcherThread.Join(10000);

         try
         {
            dispatcherThread.Abort();
         }
         catch(Exception e)
         {
            Console.WriteLine("LogZentrale.Stop {0}", e.Message);
         }
         
         return true;
      }

      private void DispatcherThreadFunc()
      {
         LogEintrag logEintrag;

         while (!terminate)
         {
            while ((logEintraege.Count != 0) && (!terminate))
            {
               try
               {
                  logEintrag = (LogEintrag)logEintraege.Dequeue();

                  Dispatch(logEintrag);
               }
               catch (Exception e7)
               {
                  Console.WriteLine("Exception in LogZentrale.DispatcherThreadFunc -> {0}", e7.Message);
               }
            }

            if ((!terminate) && (logEintraege.Count == 0))
            {
               lock (logEintraege)
               {
                  if (logEintraege.Count == 0)
                  {
                     Monitor.Wait(logEintraege);
                  }
               }
            }
         }
         while (logEintraege.Count != 0)
         {
            try
            {
               logEintrag = (LogEintrag)logEintraege.Dequeue();

               Dispatch(logEintrag);
            }
            catch(Exception e8)
            {
               Console.WriteLine("Exception in LogZentrale.DispatcherThreadFunc2 -> {0}", e8.Message);
            }
         }
      }

      public void Log(LogEintrag logEintrag)
      {
         logEintraege.Enqueue(logEintrag);

         lock (logEintraege)
         {
            Monitor.PulseAll(logEintraege);
         }
      }

      private void Dispatch(LogEintrag logEintrag)
      {
         if(writeEvent != null)
         {
            //Für alle registrierten Member wird deren Write Funktion aufgerufen
            //Aufruf aus DispatcherThreadFunc
            
            if(logEintrag.LogFlags == ELF.ERROR)
            {
               //Console.Beep(800, 50);
               //Console.Beep(300, 50);
            }
            
            writeEvent(logEintrag);
         }
      }

      
      public void Register(WriteDelegate writeDelegate)
      {
         writeEvent += writeDelegate;
      }
   }
}
