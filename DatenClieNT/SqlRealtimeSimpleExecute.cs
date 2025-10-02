
using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Threading;

namespace DatenClieNT
{
   public class SqlRealtimeSimpleExecute : SqlRealtime
   {
      private string stmt;
      private string database;
            
            
      public SqlRealtimeSimpleExecute(string database, int timeout, string formatString, params object[] paramObjects)
      {
         this.database = database;
         this.stmt = string.Format(formatString, paramObjects);

         InnerExecute(timeout);
      }

      public SqlRealtimeSimpleExecute(string database, string formatString, params object[] paramObjects)
      {
         this.database = database;
         this.stmt = string.Format(formatString, paramObjects);

         Execute();
      }


      private void Execute()
      {
         BasisExecute(InnerExecute);
      }

      private void InnerExecute(int timeout)
      {
         HiresStopUhr connectionUhr = new HiresStopUhr();
         HiresStopUhr executeUhr = new HiresStopUhr();

         connectionUhr.Start();  
         
         SqlConnection sqlConnection = TheDC.GetSingleton().GetDatabaseConnectionProvider(database).GetOpenDBConnection();

         connectionUhr.Stop();

         if (sqlConnection == null)
         {
            throw new Exception("SqlRealtimeSimpleExecute.Execute -> Störung der Datenbankverbindung !");
         }

         executeUhr.Start();

         try
         {
            SqlCommand dataCommand = new SqlCommand();
            dataCommand.Connection = sqlConnection;
            dataCommand.CommandTimeout = ConfigManager.TimeoutAsSeconds(timeout);
            dataCommand.CommandText = stmt;

            dataCommand.ExecuteNonQuery();
         }
         catch (Exception e)
         {
            if (e.Message.ToLower().Contains("timeout"))
            {
               throw new Exception("TIMEOUT");
            }
            else
            {
               throw new Exception(string.Format("Exception in SqlRealtimeSimpleExecute.ExecuteNonQuery -> {0}", e.Message));
            }
         }
         finally
         {
            sqlConnection.Close();
         }

         executeUhr.Stop();

         if ((connectionUhr.PeriodMilliSeconds + executeUhr.PeriodMilliSeconds) > TheDC.DatabaseCriticalDelay)
         {
            LogManager.GetSingleton().ZLog("CD071", ELF.WARNING, "DB-Zugriff dauerte länger als {3}ms -> Statement=|{0}| DauerConnect={1} DauerExecute={2}", stmt, connectionUhr.PeriodMilliSeconds, executeUhr.PeriodMilliSeconds, TheDC.DatabaseCriticalDelay);
         }

         LogManager.GetSingleton().ZLog("CD204", ELF.DEVELOPER, "DB-DELAY {0}ms {1}", connectionUhr.PeriodMilliSeconds + executeUhr.PeriodMilliSeconds, stmt);
      }
   }
}
