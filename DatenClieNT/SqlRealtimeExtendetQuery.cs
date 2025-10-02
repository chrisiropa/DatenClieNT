using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using System.Threading;

namespace DatenClieNT
{
   public class SqlRealtimeExtendetQuery : SqlRealtime
   {
      private string database;
      private string query;
      private Exception exception = null;
      private Dictionary<string, object> parameters = new Dictionary<string, object>();

      public Exception Exception
      {
         get { return exception; }
      }

      private List<Dictionary<string, object>> queryResult = new List<Dictionary<string, object>>();

      public List<Dictionary<string, object>> QueryResult
      {
         get { return queryResult; }
      }

      public SqlRealtimeExtendetQuery(string database, string formatString, params object[] paramObjects)
      {
         this.database = database;
         this.query = string.Format(formatString, paramObjects);
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
      
      public void Execute()
      {
         BasisExecute(InnerExecute);
      }

      public void Execute(int timeout)
      {
         InnerExecute(timeout);
      }

      private void InnerExecute(int timeout)
      {
         HiresStopUhr connectionUhr = new HiresStopUhr();
         HiresStopUhr executeUhr = new HiresStopUhr();

         SqlDataReader dataReader = null;

         connectionUhr.Start();

         SqlConnection sqlConnection = TheDC.GetSingleton().GetDatabaseConnectionProvider(database).GetOpenDBConnection();

         connectionUhr.Stop();

         if (sqlConnection == null)
         {
            throw new Exception("SqlRealtimeExtendetQuery.Execute: Störung der Datenbankverbindung !");
         }

         try
         {
            SqlCommand dataCommand = new SqlCommand();
            dataCommand.Connection = sqlConnection;
            dataCommand.CommandTimeout = ConfigManager.TimeoutAsSeconds(timeout);
            dataCommand.CommandText = query;

            foreach (string name in parameters.Keys)
            {
               dataCommand.Parameters.AddWithValue(name, parameters[name]);
            }

            executeUhr.Start();

            dataReader = dataCommand.ExecuteReader();
            
            while (dataReader.Read())
            {
               Dictionary<string, object> dict = new Dictionary<string, object>();

               for (int i = 0; i < dataReader.FieldCount; i++)
               {
                  string fieldName = dataReader.GetName(i);
                  object value = dataReader[fieldName];
                  dict[fieldName] = value;
               }

               queryResult.Add(dict);
            }

            dataReader.Close();
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
               throw new Exception(string.Format("Exception in SqlRealtimeExtendetExecute.InnerExecute -> {0}", e.Message));
            }
         }
         finally
         {
            if (dataReader != null)
            {
               dataReader.Close();
            }

            sqlConnection.Close();
         }

         executeUhr.Stop();

         if ((connectionUhr.PeriodMilliSeconds + executeUhr.PeriodMilliSeconds) > TheDC.DatabaseCriticalDelay)
         {
            LogManager.GetSingleton().ZLog("CD21C", ELF.WARNING, "DB-Zugriff dauerte länger als {3}ms -> Statement=|{0}| DauerConnect={1} DauerExecute={2}", query, connectionUhr.PeriodMilliSeconds, executeUhr.PeriodMilliSeconds, TheDC.DatabaseCriticalDelay);
         }

         LogManager.GetSingleton().ZLog("CD21D", ELF.DEVELOPER, "DB-DELAY {0}ms {1}", connectionUhr.PeriodMilliSeconds + executeUhr.PeriodMilliSeconds, query);
      }
   }
}
