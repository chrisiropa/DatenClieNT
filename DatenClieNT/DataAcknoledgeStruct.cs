using System;
using System.Collections.Generic;
using System.Text;

namespace DatenClieNT
{
   public class DataAcknoledge
   {
      private UInt16 indexTelegram;
      private SpsReturnCodes spsReturnCode;

      public DataAcknoledge(UInt16 indexTelegram, SpsReturnCodes spsReturnCode)
      {
         this.indexTelegram = indexTelegram;
         this.spsReturnCode = spsReturnCode;
      }

      public void SetTelegramIndex(UInt16 indexTelegram)
      {
         this.indexTelegram = indexTelegram;
      }

      public void SetSpsReturnCode(SpsReturnCodes spsReco)
      {
         this.spsReturnCode = spsReco;
      }

      public UInt16 GetTelegramIndex()
      {
         return indexTelegram;
      }

      public SpsReturnCodes GetSpsReturnCode()
      {
         return this.spsReturnCode;
      }

      
   }
}
