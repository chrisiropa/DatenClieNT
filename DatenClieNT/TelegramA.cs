using System;
using System.Collections.Generic;
using System.Text;

namespace DatenClieNT
{
   public class TelegramA : Telegram
   {
      private Int64 auftragsID = -1;
      private Int64 auftragsnummer = -1;

      public TelegramA(string subscriptionItem, Dictionary<string, string> registrationItems)
      : base(subscriptionItem, registrationItems)
      {
         teleType = "A";
         
         auftragsnummer = -1;
      }

      public long Auftragsnummer
      {
         set { auftragsnummer = value; }
         get { return auftragsnummer; }
      }

      public long AuftragsID
      {
         set { auftragsID = value; }
         get { return auftragsID; }
      }

      public override Telegram DeepCopy()
      {
         TelegramA telegram = (TelegramA)Telegram.NewInstance("", null, "A");

         telegram.errorText = ErrorText;
         telegram.Counter = this.Counter;

         telegram.auftragsnummer = this.auftragsnummer;
         telegram.auftragsID = this.auftragsID;
         
         
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
