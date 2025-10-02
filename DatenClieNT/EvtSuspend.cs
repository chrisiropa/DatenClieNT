using System;
using System.Collections.Generic;
using System.Text;

namespace DatenClieNT
{
   public class EvtSuspend : Evt
   {
      public EvtSuspend()
         : base(Type.SuspendPiping)
      {

      }

      public override string ToString()
      {
         return string.Format("Suspend Event");
      }
   }
}