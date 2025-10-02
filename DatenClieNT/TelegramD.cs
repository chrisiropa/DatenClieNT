using System;
using System.Collections.Generic;
using System.Text;

namespace DatenClieNT
{
   public class TelegramD : Telegram
   {
      private UInt16 indexTelegramD = 0;

      public UInt16 IndexTelegramD
      {
         set { indexTelegramD = value; }
         get { return indexTelegramD; }
      }

      public TelegramD(string subscriptionItem, Dictionary<string, string> registrationItems)
         : base(subscriptionItem, registrationItems)
      {
         teleType = "D";
         indexTelegramD = 0;
      }


      public override Telegram DeepCopy()
      {
         TelegramD telegram = (TelegramD)Telegram.NewInstance("", null, "D");

         telegram.indexTelegramD = this.indexTelegramD;

         telegram.errorText = ErrorText;
         
         telegram.Counter = this.Counter;
         
         telegram.TeleType = this.TeleType;
         telegram.Active = this.Active;
         telegram.Available = this.Available;
         telegram.Readable = this.Readable;
         telegram.ByteLen = this.ByteLen;

         telegram.SubscriptionItem = this.SubscriptionItem;

         telegram.SetRegistrationItems(this.registrationItems);

         return telegram;
      }
   }
}

