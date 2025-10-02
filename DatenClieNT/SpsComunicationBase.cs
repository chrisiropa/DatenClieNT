using System;
using System.Collections.Generic;
using System.Text;

namespace DatenClieNT
{
   public abstract class SpsComunicationBase
   {
      public enum ComType
      {
         OpcUA = 0
      }

      protected string database;
      protected Int64 datenClientID;
      protected Int64 anlagenID;
      protected SystemType systemType;
      protected TelegramManager telegramManager;
      protected Boolean running = false;
      protected Boolean allTelegramsAvailable = false;
      protected EventDelegate eventDelegate = null;

      public static SpsComunicationBase ProduceOPC(Dictionary<string, Auftrag> spsAuftraege, Dictionary<string, Auftrag> clientAuftraege, long anlagenID, string database, SystemType systemType, EventDelegate eventDelegate, string opcServerName, string opcRechnerName, long datenClientID, string dcName, int startOffsetArrays, Boolean standardAlarming)
      {
         return SpsCommunicationOPC.NewInstance(spsAuftraege, clientAuftraege, anlagenID, database, systemType, eventDelegate, opcServerName, opcRechnerName, datenClientID, dcName, startOffsetArrays, standardAlarming);
      }


      public SpsComunicationBase(string database)
      {
         this.database = database;
      }

      public Boolean Running
      {
         get { return running; }
      }

      public virtual void SpsUnavailable()
      {

      }

      public virtual Boolean Critical()
      {
         return false;
      }


      public virtual void RegularStartDone()
      {

      }

      public abstract Boolean Start();
      public abstract void Stop();

      public abstract long ConnectDelay
      {
         get;
      }


      public abstract Boolean AllTelegramsAvailable
      {
         get;
      }

      public abstract Boolean ConnectComplete();
      protected abstract void ConnectTelegram(Telegram telegram);

      public abstract void SendTelegram(Telegram telegram);
      public abstract void ForceTelegram(string teleType, object tag);


      public virtual Telegram NewTelegramInstance(string teleType)
      {
         return telegramManager.NewTelegramInstance(teleType);
      }

      public virtual Telegram GetReferenceInstance(string teleType)
      {
         return telegramManager.GetReferenceInstance(teleType);
      }



      protected Boolean CheckTelegramsAvailable()
      {
         Boolean allAvailable = true;

			Dictionary<string, Telegram> t = new Dictionary<string, Telegram>();
			

         foreach (Telegram telegram in telegramManager.Telegrams.Values)
         {
            if (!telegram.Available)
            {
               ConnectTelegram(telegram);

               if (!telegram.Available)
               {
                  //Nochmal genau hier weitermachen bei nächstem Timer-Event
                  return false;
               }
            }

            if (!telegram.Available)
            {
               allAvailable = false;
            }
         }

         return allAvailable;
      }

		public abstract bool MonitoredItemsCreated(Boolean mitLoggen);
	}
}
