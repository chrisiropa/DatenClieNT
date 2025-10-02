using System;
using System.Collections.Generic;
using System.Text;

namespace DatenClieNT
{
   public class EvtResume : Evt
   {
      public EvtResume()
         : base(Type.ResumePiping)
      {

      }

      public override string ToString()
      {
         return string.Format("Resume Event");
      }
   }
}