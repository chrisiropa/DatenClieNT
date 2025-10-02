using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;


namespace DatenClieNT_MaxLogID
{
   class Program
   {
      [STAThread]
      static void Main(string[] args)
      {
         string searchPath = @"C:\Projekte\IROPA\Datenclient TIA\DatenClieNT";
         string searchKrit1 = "LogManager.GetSingleton().ZLog(\"C";
			string searchKrit2 = "LogManager.GetSingleton().ZLog(\"D";

         DirectoryWalker dw = new DirectoryWalker();
         dw.Walk(searchPath, "", "*.cs");

         List<int> ids = new List<int>();

         foreach (string path in dw.Paths)
         {
            string[] lines = File.ReadAllLines(path);
            
            foreach(string line in lines)
            {
               if(line.Contains(searchKrit1))
               {
                  ids.Add(Convert.ToInt32(line.Trim().Split('"')[1], 16));
               }
					if(line.Contains(searchKrit2))
               {
                  ids.Add(Convert.ToInt32(line.Trim().Split('"')[1], 16));
               }
            }  
         }
         
         ids.Sort();
         
         int lastID = -1;
         foreach(int id in ids)
         {
            if(lastID == id)
            {
               Console.WriteLine("Doppelt -> {0}", string.Format("{0:X}",id));
            }
            lastID = id;
         }

         string next = string.Format("{0:X}", 1 + ids.Max());

         Console.WriteLine("NextID = {0} befindet sich in der Zwischenablage !", next);
         
         Clipboard.SetText(next);
         
         Console.ReadLine();
      }
   }
}
