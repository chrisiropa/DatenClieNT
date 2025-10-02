using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;

namespace DatenClieNT
{
   class DbTools
   {
      public static string GetString(Dictionary<string, object> prm, string key)
      {
         string ret = "";

         try
         {
            if (!prm[key].Equals(System.DBNull.Value))
            {
               ret = (string)prm[key];
            }
            else
            {
               ret = "";
            }
         }
         catch (Exception e)
         {
            throw new Exception(string.Format("Exception in DbTools.GetString({0}) -> {1}", key, e.Message));
         }

         return ret;
      }

      public static Boolean GetBoolean(Dictionary<string, object> prm, string key)
      {
         Boolean ret = false;

         try
         {
            if (!prm[key].Equals(System.DBNull.Value))
            {
               ret = (Boolean)prm[key];
            }
            else
            {
               ret = false;
            }
         }
         catch (Exception e)
         {
            throw new Exception(string.Format("Exception in DbTools.GetBoolean({0}) -> {1}", key, e.Message));
         }

         return ret;
      }

      public static long GetInt64(Dictionary<string, object> prm, string key)
      {
         long ret = 0;

         try
         {
            if (!prm[key].Equals(System.DBNull.Value))
            {
               ret = Convert.ToInt64(prm[key]);
            }
            else
            {
               ret = -1;
            }
         }
         catch (Exception e)
         {
            throw new Exception(string.Format("Exception in DbTools.GetInt64({0}) -> {1}", key, e.Message));
         }

         return ret;
      }

      public static int GetInt32(Dictionary<string, object> prm, string key)
      {
         int ret = 0;

         try
         {
            if (!prm[key].Equals(System.DBNull.Value))
            {
               ret = (int)prm[key];
            }
            else
            {
               ret = -1;
            }
         }
         catch (Exception e)
         {
            throw new Exception(string.Format("Exception in DbTools.GetInt32({0}) -> {1}", key, e.Message));
         }

         return ret;
      }

      public static double GetDouble(Dictionary<string, object> prm, string key)
      {
         double ret = 0;

         try
         {
            if (!prm[key].Equals(System.DBNull.Value))
            {
               ret = (double)prm[key];
            }
            else
            {
               ret = 0.0;
            }
         }
         catch (Exception e)
         {
            throw new Exception(string.Format("Exception in DbTools.GetDouble({0}) -> {1}", key, e.Message));
         }

         return ret;
      }
   }
}
