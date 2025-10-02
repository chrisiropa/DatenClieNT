using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using System.Threading;

namespace DatenClieNT
{
   public class SqlRealtimeExtendetExecute : SqlRealtime
   {
      private string database = null;
      private SqlConnection sqlConnection = null;
      private string stmt;
      private Exception exception = null;
      private Dictionary<string, object> parameters = new Dictionary<string,object>();

      public Exception Exception
      {
         get { return exception; }
      }

      private List<Dictionary<string, object>> queryResult = new List<Dictionary<string, object>>();

      public List<Dictionary<string, object>> QueryResult
      {
         get { return queryResult; }
      }

      public SqlRealtimeExtendetExecute(string database, string formatString, params object[] paramObjects)
      {
         this.database = database;
         this.stmt = string.Format(formatString, paramObjects);
      }


      public SqlRealtimeExtendetExecute(SqlConnection sqlConnection, string stmt)
      {
         this.stmt = stmt;
         this.sqlConnection = sqlConnection;
      }

      
      public void AddParameter(string name, object value)
      {
         parameters[name] = value;
      }
      
      public void SetPseudoVars(Auftrag auftrag)
      {
         foreach (string pseudoVariable in auftrag.PseudoScript.VariablenListe.Values)
         {
            string parameterName = pseudoVariable;

            object value = null;

            if (pseudoVariable == "@DC_UTC_HIRES")
            {
               //100 Nanosekunden seit dem 01.01.1601
               value = DateTime.UtcNow.ToFileTimeUtc();
            }
            else if (pseudoVariable == "@DC_UTC_DATETIME")
            {
               value = DateTime.UtcNow;
            }
            else if (pseudoVariable == "@DC_LOCAL_DATETIME")
            {
               value = DateTime.Now;
            }
            else if (pseudoVariable == "@ANLAGEN_ID")
            {
               value = auftrag.AnlagenID;
            }
            else if (pseudoVariable == "@TAG")
            {
               value = auftrag.Tag;
            }

            parameters[parameterName] = value;
         }
      }
      
      public void ExecuteNonQueryRecycleSqlConnection()
      {
         BasisExecute(InnerExecuteNonQueryRecycleSqlConnection);
      }
         
      public void ExecuteNonQuery()
      {
         BasisExecute(InnerExecuteNonQuery);
      }

      public void ExecuteNonQueryRecycleSqlConnection(int timeout)
      {
         InnerExecuteNonQueryRecycleSqlConnection(timeout);
      }

      public void ExecuteNonQuery(int timeout)
      {
         InnerExecuteNonQuery(timeout);
      }


      private void InnerExecuteNonQuery(int timeout)
      {
         HiresStopUhr connectionUhr = new HiresStopUhr();
         HiresStopUhr executeUhr = new HiresStopUhr();

         connectionUhr.Start();  
         
         SqlConnection sqlConnection = TheDC.GetSingleton().GetDatabaseConnectionProvider(database).GetOpenDBConnection();

         connectionUhr.Stop();  

         if (sqlConnection == null)
         {
            throw new Exception("SqlRealtimeExtendetExecute.ExecuteNonQuery: Störung der Datenbankverbindung !");
         }

         executeUhr.Start();

         try
         {
            SqlCommand dataCommand = new SqlCommand();
            dataCommand.Connection = sqlConnection;
            dataCommand.CommandTimeout = ConfigManager.TimeoutAsSeconds(timeout);
            dataCommand.CommandText = stmt;

            
            foreach (string name in parameters.Keys)
            {
               dataCommand.Parameters.AddWithValue(name, parameters[name]);
            }

            dataCommand.ExecuteNonQuery(); 
         }
         catch (Exception e)
         {
            queryResult = null;
            exception = e;
            
            if(e.Message.ToLower().Contains("timeout"))
            {
               throw new Exception("TIMEOUT");
            }
            else
            {
               throw new Exception(string.Format("Exception in SqlRealtimeExtendetExecute.ExecuteNonQuery -> {0}", e.Message));
            }
         }
         finally
         {
            sqlConnection.Close();
         }

         executeUhr.Stop();

         if ((connectionUhr.PeriodMilliSeconds + executeUhr.PeriodMilliSeconds) > TheDC.DatabaseCriticalDelay)
         {
            LogManager.GetSingleton().ZLog("CD21A", ELF.WARNING, "DB-Zugriff dauerte länger als {3}ms -> Statement=|{0}| DauerConnect={1} DauerExecute={2}", stmt, connectionUhr.PeriodMilliSeconds, executeUhr.PeriodMilliSeconds, TheDC.DatabaseCriticalDelay);
         }

         LogManager.GetSingleton().ZLog("CD21B", ELF.DEVELOPER, "DB-DELAY {0}ms {1}", connectionUhr.PeriodMilliSeconds + executeUhr.PeriodMilliSeconds, stmt);
      }

      private void InnerExecuteNonQueryRecycleSqlConnection(int timeout)
      {
         HiresStopUhr executeUhr = new HiresStopUhr();
         HiresStopUhr timeoutWatch = new HiresStopUhr();

         executeUhr.Start();

         try
         {
            SqlCommand dataCommand = new SqlCommand();
            dataCommand.Connection = sqlConnection;
            dataCommand.CommandTimeout = ConfigManager.TimeoutAsSeconds(timeout);
            dataCommand.CommandText = stmt;

            foreach (string name in parameters.Keys)
            {
               dataCommand.Parameters.AddWithValue(name, parameters[name]);
            }

            timeoutWatch.Start();

            dataCommand.ExecuteNonQuery();

            timeoutWatch.Stop();    
         }
         catch (Exception e)
         {
            queryResult = null;
            exception = e;

            if (e.Message.ToLower().Contains("timeout"))
            {
               throw new Exception("TIMEOUT");
            }
            else
            {
               throw new Exception(string.Format("Exception in SqlRealtimeExtendetExecute.InnerExecuteNonQueryRecycleSqlConnection -> {0}", e.Message));
            }
         }
         
         executeUhr.Stop();

         if (executeUhr.PeriodMilliSeconds > TheDC.DatabaseCriticalDelay)
         {
            LogManager.GetSingleton().ZLog("CD21E", ELF.WARNING, "DB-Zugriff dauerte länger als {2}ms -> Statement=|{0}| DauerExecute={1}", stmt, executeUhr.PeriodMilliSeconds, TheDC.DatabaseCriticalDelay);
         }

         LogManager.GetSingleton().ZLog("CD21F", ELF.DEVELOPER, "DB-DELAY {0}ms {1}", executeUhr.PeriodMilliSeconds, stmt);
      }
   }
}
