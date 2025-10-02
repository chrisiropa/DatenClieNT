using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.IO;

namespace DatenClieNT
{
   public static class AccountInfo
   {
      [Flags]
      enum TOKEN_ACCESS : uint
      {
         TOKEN_QUERY = 0x0008
      };


      [DllImport("Advapi32.dll", SetLastError = true)]
      extern static int OpenProcessToken(IntPtr processHandle, TOKEN_ACCESS desiredAccess, out IntPtr tokenHandle);

      [DllImport("kernel32.dll", SetLastError = true)]
      extern static bool CloseHandle(IntPtr handle);

      public static string CurrentUser()
      {
         string currentUser = "undef.";


         try
         {
            Process process = System.Diagnostics.Process.GetCurrentProcess();
            
            
            
            IntPtr token = IntPtr.Zero;
            if (OpenProcessToken(process.Handle, TOKEN_ACCESS.TOKEN_QUERY, out token) != 0)
            {
               WindowsIdentity who = new WindowsIdentity(token);
               CloseHandle(token);

               currentUser = who.Name;
            }
         }
         catch (Exception)
         {
         }

         return currentUser;
      }
      
      public static Boolean IsService()
      {
         Boolean service = false;
         
         try
         {
            Process process = System.Diagnostics.Process.GetCurrentProcess();
            IntPtr token = IntPtr.Zero;
            if (OpenProcessToken(process.Handle, TOKEN_ACCESS.TOKEN_QUERY, out token) != 0)
            {
               WindowsIdentity who = new WindowsIdentity(token);
               CloseHandle(token);
               
               if(who.IsSystem)
               {
                  service = true;
               }
            }
         }
         catch (Exception)
         {
         }

         return service;

      }
   }
}



