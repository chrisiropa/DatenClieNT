using System;
using System.Collections.Generic;
using System.Text;

namespace DatenClieNT
{
   public class Watchdog
   {
      private long counter = 0;
      private DateTime lastWatchdogUTC = DateTime.MinValue;
      private long delayInMilliSeconds = 0;

      private DateTime lastTimerWatchdogUTC = DateTime.MinValue;
      private long delayInMilliSecondsForTimeout = 0;



      private Boolean initialTelegramNotified = false;

      public Boolean TimedOut
      {
			//Dies ist nicht der Timeout, der zu einem Neustart führt, sondern nur der, das der Watchdog nicht zu häufig gemeldet wird.
         get
         {
            TimeSpan timeSpan = DateTime.UtcNow - lastTimerWatchdogUTC;

            delayInMilliSecondsForTimeout = (long)timeSpan.TotalMilliseconds;

            if (timeSpan.TotalMilliseconds > 1000)
            {
               //Erst wenn die Sekunde abgelaufen ist, tätig werden
               //Da der Timer hochfrequenter kommt als 1000 ms.

               lastTimerWatchdogUTC = DateTime.UtcNow;
               return true;
            }

            return false;
         }
      }
      
      public Boolean Ok
      {
         get
         {
         
         
            //return false;
         
         
            TimeSpan timeSpan = DateTime.UtcNow - lastWatchdogUTC;

            delayInMilliSeconds = (long) timeSpan.TotalMilliseconds;

            if (timeSpan.TotalMilliseconds > 10000)
            {
               //Wenn länger als 10 Sekunden kein WD gekommen ist, ist die SPS Verbindung gestört
               //Es muß bedacht werden, das der WD durch die Pipe muß und somit verzögert eintreffen kann
               return false;
            }

            return true;
         }
      }
      
          
      public void Start()
      {
         counter = 0;
         lastWatchdogUTC = DateTime.MinValue;
         initialTelegramNotified = false;
      }
      
      
      
      public void Stop()
      {
      
      }

      public Boolean NotifyTelegram()
      {
         Boolean firstWatchdog = false;
         //Das erste eintreffende Telegram nicht mitzählen, da es das Initiale vom OPC-Server ist
         //Da wird einfach der Wert genommen, der in der SPS steht.
         //Das heißt noch nicht, daß der Watchdog läuft.
         if(initialTelegramNotified)
         {
            counter++;
            lastWatchdogUTC = DateTime.UtcNow;
            
            if(counter == 1)
            {
               firstWatchdog = true;            
            }
         }
         
         initialTelegramNotified = true;

         return firstWatchdog;
      }
      
      
   }
}
