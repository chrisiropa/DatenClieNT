using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace DatenClieNT
{
   public delegate void ExecuteDelegate(int timeout);

   public class SqlRealtime
   {
   
      protected void BasisExecute(ExecuteDelegate executeDelegate)
      {
         try
         {
            executeDelegate(ConfigManager.FirstTimeout);
            return;
         }
         catch (Exception e1)
         {
            if(e1.Message.ToUpper().Contains("TIMEOUT"))
            {
               LogManager.GetSingleton().ZLog("CD243", ELF.WARNING, "DB: First Attempt FAILED -> TIMEOUT");
            }
            else
            {
               //Nicht weiter versuchen !
               //Kein Timeout
               //LogManager.GetSingleton().ZLog("CD248", ELF.WARNING, "DB: First Attempt FAILED -> {0}", e1.Message);
               //Keine Fehlermeldung, wird von dem geloggt, der die nächste Exception fängt, wenn es wichtig ist
               throw e1;
            }
            
         }

         Thread.Sleep(10);
         //2.Mal versuchen !
         //1.Versuch war Timeout

         try
         {
            executeDelegate(ConfigManager.SecondTimeout);
            LogManager.GetSingleton().ZLog("CD233", ELF.WARNING, "DB: Second Attempt SUCCESS");
            return;
         }
         catch(Exception e1)
         {
            LogManager.GetSingleton().ZLog("CD240", ELF.WARNING, "DB: Second Attempt FAILED -> {0}", e1.Message);
            throw e1;
         }
      }
   }
}
