using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using System.Threading;

namespace DatenClieNT
{
   public class SqlRealtimeSimpleQuery : SqlRealtime
   {
      private string database;
      private string query;
      private Exception exception = null;
      
      
      public string Statement
      {
         get { return query; }
      }

      public Exception Exception
      {
         get { return exception; }
      }

      private List<Dictionary<string, object>> queryResult = new List<Dictionary<string, object>>();

      public List<Dictionary<string, object>> QueryResult
      {
         get { return queryResult; }
      }

      public SqlRealtimeSimpleQuery(string database, string formatString, params object[] paramObjects)
      {
         this.database = database;
         this.query = string.Format(formatString, paramObjects);
         BasisExecute(InnerExecute);
      }


      public SqlRealtimeSimpleQuery(int timeout, string database, string formatString, params object[] paramObjects)
      {
         this.database = database;
         this.query = string.Format(formatString, paramObjects);
         
         //Bei definiertem Timeout wird direkt und dann nur einmalig die 
         //ausführende Funktion aufgerufen
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
            throw new Exception("SqlRealtimeSimpleQuery.Execute: Störung der Datenbankverbindung !");
         }

         try
         {
            SqlCommand dataCommand = new SqlCommand();
            dataCommand.Connection = sqlConnection;
            dataCommand.CommandTimeout = ConfigManager.TimeoutAsSeconds(timeout);
            dataCommand.CommandText = query;

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
               throw new Exception(string.Format("Exception in SqlRealtimeSimpleQuery.InnerExecute -> {0}", e.Message));
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
            LogManager.GetSingleton().ZLog("CD246", ELF.WARNING, "DB-Zugriff dauerte länger als {3}ms -> Statement=|{0}| DauerConnect={1} DauerExecute={2}", query, connectionUhr.PeriodMilliSeconds, executeUhr.PeriodMilliSeconds, TheDC.DatabaseCriticalDelay);
         }

         LogManager.GetSingleton().ZLog("CD247", ELF.DEVELOPER, "DB-DELAY {0}ms {1}", connectionUhr.PeriodMilliSeconds + executeUhr.PeriodMilliSeconds, query);
      }
   }
}
