using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Data.SqlClient;

namespace DatenClieNT
{
   public class DatabaseConnectionProvider
   {
      private Boolean databaseConnectionOk;
      private string database = "";

      public DatabaseConnectionProvider(string database)
      {
         this.database = database;
         this.databaseConnectionOk = false;
      }

      public SqlConnection GetLowPrioOpenDBConnection()
      {
         if (databaseConnectionOk)
         {
            SqlConnection sqlConnection = null;

            if (DatabaseOk(out sqlConnection))
            {
               databaseConnectionOk = true;
               return sqlConnection;
            }

            //Bei LowPrio-Abfragen (Konfigurationsänderung usw.)
            //nicht sofort das gestört-Flag bemühen
            //databaseConnectionOk = false;
            LogManager.GetSingleton().ZLog("CD214", ELF.INFO, "GetLowPrioOpenDBConnection -> Datenbankverbindung gestört !");
         }

         return null;
      }

      public SqlConnection GetOpenDBConnection()
      {
         if (databaseConnectionOk)
         {
            SqlConnection sqlConnection = null;

            if (DatabaseOk(out sqlConnection))
            {
               databaseConnectionOk = true;
               return sqlConnection;
            }

            databaseConnectionOk = false;
         }

         LogManager.GetSingleton().ZLog("C0063", ELF.ERROR, "GetOpenDBConnection -> Datenbankverbindung als gestört markiert !");

         return null;
      }

      public SqlConnection ForceDatabaseConnection()
      {
         SqlConnection sqlConnection = null;

         LogManager.GetSingleton().ZLog("C0064", ELF.INFO, "DatabaseConnectionProvider -> Versuche DB-Verbindung zu erzwingen...");

         if (DatabaseOk(out sqlConnection))
         {
            databaseConnectionOk = true;
            LogManager.GetSingleton().ZLog("C0065", ELF.INFO, "DatabaseConnectionProvider -> ... SUCCESS");
            return sqlConnection;
         }

         databaseConnectionOk = false;
         LogManager.GetSingleton().ZLog("C0066", ELF.INFO, "DatabaseConnectionProvider -> ... FAILED");
         return null;
      }

      private Boolean DatabaseOk(out SqlConnection sqlConnection)
      {
         Boolean dbOk = false;
         sqlConnection = null;
         
         try
         {
            string connectionString = ConfigManager.GetSingleton().SpecificConnectionString(database);

            sqlConnection = new SqlConnection(connectionString);            
            sqlConnection.Open();
            
            dbOk = true;

         }
         catch (Exception e)
         {
            if (sqlConnection != null)
            {
               sqlConnection.Close();
            }

            LogManager.GetSingleton().ZLog("C0067", ELF.ERROR, "DatabaseConnectionProvider.DatabaseOk(Open FAILED) -> Datenbankverbindung gestört -> e={0}", e.Message);
            return dbOk;
         }
         
         return dbOk;
      }
   }
}
