using System;
using System.Collections.Generic;
using System.Text;

namespace DatenClieNT
{
   public class TelegramE : Telegram
   {
      //Passives Telegram: Anreiz TelegramD
      //Daten lesen von SPS/OPC

      private UInt16 indexTelegramD = 0;
      private UInt16 indexTelegramE = 0;
      private Int64 auftragsnummer = -1;

      public TelegramE(string subscriptionItem, Dictionary<string, string> registrationItems)
      : base(subscriptionItem, registrationItems)
      {
         teleType = "E";

         indexTelegramD = 0;
         indexTelegramE = 0;
         auftragsnummer = -1;
      }

      public long Auftragsnummer
      {
         //indexTelegramD ist richtig ! Es handelt sich hier um den Anreiz, der
         //mit dem D-Telegramm gekommen ist und hier gespeichert wird.
         set { auftragsnummer = value; }
         get { return auftragsnummer; }
      }

      public UInt16 IndexTelegramD
      {
         //indexTelegramD ist richtig ! Es handelt sich hier um den Anreiz, der
         //mit dem D-Telegramm gekommen ist und hier gespeichert wird.
         set { indexTelegramD = value; }
         get { return indexTelegramD; }
      }

      public UInt16 IndexTelegramE
      {
         set { indexTelegramE = value; }
         get { return indexTelegramE; }
      }

     
      public override Telegram DeepCopy()
      {
         TelegramE telegram = (TelegramE)Telegram.NewInstance("", null, "E");

         telegram.auftragsnummer = this.auftragsnummer;
         telegram.indexTelegramD = this.indexTelegramD;
         telegram.indexTelegramE = this.indexTelegramE;

         telegram.Counter = this.Counter;
         
         telegram.errorText = ErrorText;
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
