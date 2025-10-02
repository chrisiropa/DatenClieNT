using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace DatenClieNT_MaxLogID
{
   public class DirectoryWalker
   {
      public delegate void ProcessDirCallback(DirectoryInfo dir, int level, object obj);
      public delegate void ProcessFileCallback(FileInfo file, int level, object obj);

      ProcessDirCallback dirCallback;
      ProcessFileCallback fileCallback;
      
      private List<string> dirs = new List<string>();
      private List<string> paths = new List<string>();

      public List<string> Directories
      {
         get { return dirs; }
      }

      public List<string> Paths
      {
         get { return paths; }
      }  
      
      
      public DirectoryWalker(ProcessDirCallback dirCallback, ProcessFileCallback fileCallback)
      {
         this.dirCallback = dirCallback;
         this.fileCallback = fileCallback;
      }

      public DirectoryWalker()
      {
         this.dirCallback = InternDirCallback;
         this.fileCallback = InternPathCallback;
      }

      private void InternDirCallback(DirectoryInfo d, int level, object obj)
      {
         dirs.Add(d.Name);
      }

      private void InternPathCallback(FileInfo f, int level, object obj)
      {
         paths.Add(f.FullName.ToLower());
      }

      
      public void Walk(string rootDir, object obj, string maske)
      {
         dirs.Clear();
         paths.Clear();
         
         DoWalk(new DirectoryInfo(rootDir), 0, obj, maske);
      }

      void DoWalk(DirectoryInfo dir, int level, object obj, string maske)
      {
         foreach (FileInfo f in dir.GetFiles(maske))
         {
            if (fileCallback != null)
            {
               fileCallback(f, level, obj);
            }
         }

         foreach (DirectoryInfo d in dir.GetDirectories())
         {
            if (dirCallback != null)
            {
               dirCallback(d, level, obj);
            }

            DoWalk(d, level + 1, obj, maske);
         }
      }
   }
}
