using System;
using System.Collections.Generic;
using System.Text;

namespace DatenClieNT
{
   public class SqlServerConnectionProvider
   {
      private Dictionary<string, DatabaseConnectionProvider> dbcs = new Dictionary<string,DatabaseConnectionProvider>();
      
      public DatabaseConnectionProvider GetDatabaseConnectionProvider(string database)
      {
         if(!dbcs.ContainsKey(database))
         {
            dbcs[database] = new DatabaseConnectionProvider(database);
         }
               
         return dbcs[database];
      }
   }
}
