using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace DatenClieNT
{
   public abstract class WorkerThreadWrapper
   {
      private string modulBezeichnung = null;   
      public delegate void WorkerThreadFuncDelegate();
   
      protected Thread workerThread = null;
      protected volatile bool terminate = false;
      
      public Int32 StandardModulStopTimeout
      {
         get
         {
            string standardModulStopTimeout = "10000";
            Int32 stdModulTo = 10000;
            
            try
            {
               standardModulStopTimeout = ConfigManager.GetSingleton().GetParameter("TIMEOUT_STANDARD_MODUL_STOP", "10000");
               stdModulTo = Convert.ToInt32(standardModulStopTimeout);
            }
            catch
            {
               stdModulTo = 10000;      
            }

            return stdModulTo;     
         }         
      }
      
      public string ModulBezeichnung
      {
         get { return modulBezeichnung; }
      }

      protected WorkerThreadWrapper(string modulBezeichnung)
      {
         this.modulBezeichnung = modulBezeichnung;
      }
      
      public abstract Boolean Start();
      public abstract Boolean Stop();

      protected void RunWorkerThread(WorkerThreadFuncDelegate workerThreadFunc, string name)
      {
         terminate = false;
         
         workerThread = new Thread(new ThreadStart(workerThreadFunc));
         
         if(name == null)
         {
            workerThread.Name = workerThreadFunc.Method.Name;
         }
         else
         {
            workerThread.Name = name;
         }
         
         workerThread.Start();
      }

      protected string StopWorkerThread(Int32 timeout)
      {
         Boolean exitProcessIfFailed = false;
      
         if(timeout == 0)
         {
            exitProcessIfFailed = true;         
            timeout = (StandardModulStopTimeout * 2);
         }
      
         string errorMessage = null;
      
         if(workerThread != null)
         {      
            terminate = true;
            
            if (!workerThread.Join(timeout))
            {
               errorMessage = string.Format("Modul:{0} -> WorkerThread blieb {1}ms am Leben", modulBezeichnung, timeout);
               try
               {
                  if(exitProcessIfFailed)
                  {
                     System.Environment.Exit(0);
                  }
                  else
                  {
                     workerThread.Abort();
                  }

                  errorMessage += string.Format(", wurde aber erfolgreich abgeschossen.");
               }
               catch (Exception e)
               {
                  errorMessage += string.Format(" und auch das Abschiessen funktionierte nicht -> {0}", e.Message);

                  Console.WriteLine("WorkerThreadWrapper.StopWorkerThread -> {0}", e.Message);
               }
            }
         }
         
         workerThread = null;
         
         return errorMessage;
      }      
   }
}
