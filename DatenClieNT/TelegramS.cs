using System;
using System.Collections.Generic;
using System.Text;

namespace DatenClieNT
{
   public class TelegramS : Telegram
   {
      private int indexTelegramS = -1;	//Veränderungskennung von der SPS (Störcounter)

      public int IndexTelegramS
      {
         //indexTelegramD ist richtig ! Es handelt sich hier um den Anreiz, der
         //mit dem D-Telegramm gekommen ist und hier gespeichert wird.
         set { indexTelegramS = value; }
         get { return indexTelegramS; }
      }
      public TelegramS(string subscriptionItem, Dictionary<string, string> registrationItems)
         : base(subscriptionItem, registrationItems)
      {
         teleType = "S";
      }


      public override Telegram DeepCopy()
      {
         TelegramS telegram = (TelegramS)Telegram.NewInstance("", null, "S");

         telegram.Counter = this.Counter;
			indexTelegramS = this.IndexTelegramS;

         telegram.TeleType = this.TeleType;
         telegram.Active = this.Active;
         telegram.Available = this.Available;
         telegram.Readable = this.Readable;
         telegram.ByteLen = this.ByteLen;
         telegram.errorText = ErrorText;

         telegram.SubscriptionItem = this.SubscriptionItem;

         telegram.SetRegistrationItems(this.registrationItems);

         return telegram;
      }
   }
}
