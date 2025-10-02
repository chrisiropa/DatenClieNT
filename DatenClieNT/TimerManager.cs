using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace DatenClieNT
{
   public delegate void TimerDelegate(object tag);
      
   class TimerManager : WorkerThreadWrapper
   {
      private object tag;
      private EventDelegate eventDelegate = null;
      private Dictionary<int, EvtTimer> timerEvents = new Dictionary<int,EvtTimer>();
      
      private int indexKey = 0;

      private TimerManager(EventDelegate eventDelegate, object tag)
         : base("TimerManager")
      {
         this.eventDelegate = eventDelegate;
         this.tag = tag;
      }
      
      private int NextKey()
      {
         return indexKey++;
      }

      public static TimerManager NewInstance(EventDelegate eventDelegate, object tag)
      {
         return new TimerManager(eventDelegate, tag);
      }

      public void AddAbo(TimerDelegate notify, string tag, long delay, bool immediate)
      {
         EvtTimer evt = Evt.NewTimerEvt(notify, tag, true, delay, immediate);
         
         lock (timerEvents)
         {
            timerEvents[NextKey()] = (evt); 
         }
      }

      public void AddSingle(TimerDelegate notify, object tag, long delay)
      {
         EvtTimer evt = Evt.NewTimerEvt(notify, tag, false, delay, false);

			Boolean restart = false;

			try
			{
				if(tag.GetType().FullName == "System.String")
				{
					if(((string)tag) == DatenClient.ImmediateRestartTag)
					{
						restart = true;
					}
				}
			}
			catch
			{
			}

         
         lock (timerEvents)
         {
				if(restart)
				{
					//CG: 26012021
					//TimerQueue leeren, da das Restart-Event wichtiger ist. Danach geht es weiter in der eigentlichen EventQueue
					//Allerdings nicht über ein spezielles Event, sondern sondern über einen direkten Eingriff nach dem Dequeue in der EventPipe
					timerEvents.Clear();


					LogManager.GetSingleton().ZLog("D0007", ELF.WARNING, "RESTART-EVENT bewirkte ein Leeren der Timer-Queue !");
				}

            timerEvents[NextKey()] = (evt);
         }
      }
      
      

      private void TimerThread()
      {
			//Hiervon gibt es zwei Instanzen
			//1. Timer	(Pro DatenClient eine Instanz)
			//2. GlobalTimer (Konfigänderungen)

         terminate = false;

			lock(DatenClient.Threads)
			{
				DatenClient.Threads[Thread.CurrentThread.ManagedThreadId] = string.Format("{0}", tag);
			}

			

         LogManager.GetSingleton().ZLog("C0290", ELF.INFO, "TimerManager-Thread gestartet !");

         while (!terminate)         
         {
            Thread.Sleep(1);

            
            
            lock(timerEvents)
            {
               List<int> evtsToDelete = new List<int>();

               foreach(int indexKey in timerEvents.Keys)
               {
                  if(terminate)
                  {
                     break;
                  }
               
                  EvtTimer timerEvt = timerEvents[indexKey];
                  
                  if(timerEvt.TimedOut())
                  {
                     eventDelegate(timerEvt);

                     if(!timerEvt.Abo)
                     {
                        evtsToDelete.Add(indexKey);
                     }
                  }
               }
               
               //Alle einmaligen TimerEvents löschen.
               //Abbonierte bleiben drin
               foreach(int indexKey in evtsToDelete)
               {
                  timerEvents.Remove(indexKey);
               }               
            }
         }

         LogManager.GetSingleton().ZLog("C0291", ELF.INFO, "TimerManager-Thread beendet !");
      }

      public override bool Start()
      {
         base.RunWorkerThread(TimerThread, (string) tag);

         return true;
      }

      public override bool Stop()
      {
         string info = base.StopWorkerThread(base.StandardModulStopTimeout);

         if (info != null)
         {
            LogManager.GetSingleton().ZLog("C0292", ELF.ERROR, info);
         }
         
         return true;
      }
   }
}
