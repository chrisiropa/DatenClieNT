using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DatenClieNT
{
	public class VerifyLicense
	{
		private string master_MAC = "000000000000";	
		private string master_Expire = "31.12.2099 23:59:59";

		public Boolean Check(string tag)
		{
			if(Debugger.IsAttached)
			{
				return true;
         }	

         if (tag == "MAC")
			{
				string configMAC = "";
				string statement = "select count(*) as MAC_COUNT from DC_Parameter where Name like 'MAC' ";
         
				try
				{

					SqlRealtimeSimpleQuery query = new SqlRealtimeSimpleQuery(2000, ConfigManager.Database, statement);

					if (query.QueryResult != null)
					{
						Dictionary<string, object> obj = query.QueryResult[0];
						int macCount = Convert.ToInt32(obj["MAC_COUNT"]);

						if(macCount == 0)
						{
							SetMaster("MAC", master_MAC);
						}
					}
				}
				catch (Exception e)
				{  
					LogManager.GetSingleton().ZLog("C00D0", ELF.ERROR, "VerifyLicense Error = {0}", e.Message);
				}

				//Nicht implizit Valid. Dann muss die MAC Adresse aber auch richtig eingetragen sein.
				statement = "select Wert from DC_Parameter where Name like 'MAC' ";

				try
				{
				
					SqlRealtimeSimpleQuery query = new SqlRealtimeSimpleQuery(2000, ConfigManager.Database, statement);

					if (query.QueryResult != null)
					{
						Dictionary<string, object> obj = query.QueryResult[0];
						configMAC = Convert.ToString(obj["Wert"]);
					}
				}
				catch (Exception e)
				{  
					LogManager.GetSingleton().ZLog("C00D0", ELF.ERROR, "VerifyLicense Error = {0}", e.Message);
				}


				configMAC = CryptoHelper.Decrypt(configMAC);



            //Hier die MAC-Adressen überprüfen
            List<string> realMACs = MachineHelpers.GetAllMacAddresses();

            bool valid = realMACs.Any(m =>
                    m.Equals(configMAC, StringComparison.OrdinalIgnoreCase) ||
                    m.Equals(master_MAC, StringComparison.OrdinalIgnoreCase));

            if (valid)
            {
               LogManager.GetSingleton().ZLog("C00D0", ELF.INFO, "MAC korrekt: Lizenz gültig!");
               return true;
            }

            LogManager.GetSingleton().ZLog("C00D0", ELF.INFO, "MAC fehlgeschlagen: Lizenz ungültig!");

            return false;
         }			
			else if(tag == "EXPIRE")
			{	
				string expireDatum = "";
				string statement = "select count(*) as MAC_COUNT from DC_Parameter where Name like 'EXPIRE' ";
         
				try
				{

					SqlRealtimeSimpleQuery query = new SqlRealtimeSimpleQuery(2000, ConfigManager.Database, statement);

					if (query.QueryResult != null)
					{
						Dictionary<string, object> obj = query.QueryResult[0];
						int macCount = Convert.ToInt32(obj["MAC_COUNT"]);

						if(macCount == 0)
						{
							SetMaster("EXPIRE", master_Expire);
						}
					}
				}
				catch (Exception e)
				{  
					LogManager.GetSingleton().ZLog("C00D0", ELF.ERROR, "VerifyLicenseVerifyMAC Error = {0}", e.Message);
				}

				statement = "select Wert from DC_Parameter where Name like 'EXPIRE' ";

				try
				{
				
					SqlRealtimeSimpleQuery query = new SqlRealtimeSimpleQuery(2000, ConfigManager.Database, statement);

					if (query.QueryResult != null)
					{
						Dictionary<string, object> obj = query.QueryResult[0];
						expireDatum = Convert.ToString(obj["Wert"]);	//Verschlüsseltes Datum
					}
				}
				catch (Exception e)
				{  
					LogManager.GetSingleton().ZLog("C00D0", ELF.ERROR, "VerifyExpire Error = {0}", e.Message);
				}

				expireDatum = CryptoHelper.Decrypt(expireDatum);

				DateTime expire = DateTime.MinValue;
				
				try
				{
					expire = DateTime.ParseExact(expireDatum,"dd.MM.yyyy HH:mm:ss", null);
				}
				catch
				{
					LogManager.GetSingleton().ZLog("C00D0", ELF.ERROR, "Expire Failes: Lizenz ungültig da ungültiges Expire eingetragen !");
					return false;
				}

				if ((DateTime.Now > expire))
				{
					LogManager.GetSingleton().ZLog("C00D0", ELF.ERROR, "Expire Failes: Lizenz ungültig da Expire abgelaufen !");
					return false;	
				}	

				LogManager.GetSingleton().ZLog("C00D0", ELF.INFO, "DatenclieNT lizensiert bis = {0}", expire);
			}
			
			return true;
		}		

		private void SetMaster(string tag, string value)
		{
			try
         {
				value = CryptoHelper.Encrypt(value);

				string statement = string.Format("insert DC_Parameter (Name, Wert) select '{0}', '{1}'", tag, value);
            SimpleSqlExecute exec = new SimpleSqlExecute(ConfigManager.GetSingleton().MainConnectionString, statement);
            
            if(exec.Exception != null)
            {
               LogManager.GetSingleton().ZLog("C00D0", ELF.ERROR, "Verify License.SetMaster P1 error = {0}", exec.Exception);
            }
         }
         catch (Exception e)
         {
            LogManager.GetSingleton().ZLog("C00D0", ELF.ERROR, "Verify License.SetMaster P2 error = {0}", e.Message);
         }
		}
	}	
}
