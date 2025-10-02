using System;
using System.Collections.Generic;
using System.Text;

namespace DatenClieNT
{
   public class EvtTerminate : Evt
   {
      public EvtTerminate()
         : base(Type.Terminate)
      {
         
      }

      public override string ToString()
      {
         return string.Format("Terminate Event");
      }
   }
}