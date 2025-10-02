using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;

namespace DatenClieNT
{
   public class ExecuteScript
   {
      private string errorText = "";
      private string database = "";

      public string ErrorText
      {
         get { return errorText; }
      }

      public Boolean Execute(string database, string script)
      {
         Boolean success = true;

         this.database = database;
         string currentStatement = "";

         SqlConnection connection = null;
         SqlTransaction transaction = null;

         try
         {
            connection = TheDC.GetSingleton().GetDatabaseConnectionProvider(database).GetOpenDBConnection();
            transaction = connection.BeginTransaction();


            string[] commands = script.Split(new string[] { "GO\r\n", "GO ", "GO\t", "go\r\n", "go ", "go\t" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string sql in commands)
            {
               currentStatement = sql;

               SqlCommand dataCommand = new SqlCommand(sql, connection, transaction);
               dataCommand.ExecuteNonQuery();
            }
         }
         catch (Exception e)
         {
            success = false;
            errorText = string.Format("Fehler beim Ausführen eines SQL-Script-Teils: {0}\nSQL={1}", e.Message, currentStatement);
         }
         finally
         {
            try
            {
               if (success)
               {
                  transaction.Commit();
               }
               else
               {
                  transaction.Rollback();
               }

               connection.Close();
            }
            catch
            {
            }
         }

         return success;
      }
   }
}
