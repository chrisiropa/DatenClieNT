using System;
using System.Collections.Generic;

namespace DatenClieNT
{
   //public delegate void ConnectTelegramDelegate(Telegram telegram);
   //public delegate void ConnectCompleteDelegate();

   public class TelegramManager
   {
      protected long datenClientID = -1;
      protected string database;
      
      protected Dictionary<string, Telegram> telegramsByTeleType;
      protected Dictionary<string, Telegram> telegramsByItemID;

      public Dictionary<string, Telegram> Telegrams
      {
         get { return telegramsByTeleType; }
      }
   
      protected TelegramManager(string database, long datenClientID)
      {
			
         this.database = database;
         this.datenClientID = datenClientID;
      }

      public static TelegramManager NewInstance(string database, SystemType systemType, long datenClientID, int startOffsetArrays, Boolean standardAlarming)
      {
         switch(systemType)
         {
            case SystemType.TIA: return new TelegramManagerTIA(database, datenClientID, startOffsetArrays, standardAlarming);
            default: return null;
         }
      }
      
      public Telegram NewTelegramInstance(string teleType)
      {
         Telegram telegram = null;

         if (telegramsByTeleType.ContainsKey(teleType))
         {
            telegram = telegramsByTeleType[teleType];
         }
         
         if(telegram != null)
         {
            return telegram.DeepCopy();
         }

         return null;
      }

      public Telegram NewTelegramInstanceByItemID(string itemID)
      {
         Telegram telegram = null;

         if (telegramsByItemID.ContainsKey(itemID))
         {
            telegram = telegramsByItemID[itemID];
         }

         if (telegram != null)
         {
            return telegram.DeepCopy();
         }

         return null;
      }

      public Telegram GetReferenceInstance(string teleType)
      {
         Telegram telegram = null;

         if (telegramsByTeleType.ContainsKey(teleType))
         {
            telegram = telegramsByTeleType[teleType];
         }

         return telegram;
      }
      
      public virtual Boolean Start()
      {
         telegramsByTeleType = new Dictionary<string,Telegram>();
         telegramsByItemID = new Dictionary<string,Telegram>();

         LogManager.GetSingleton().ZLog("C0250", ELF.INFO, "Telegramme anmelden");
         
         return true;
       }
   }
}
