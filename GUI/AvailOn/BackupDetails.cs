using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AvailOn
{
     public class BackupDetails
    {
         private List<string> backupfilelist;

         public List<string> Backupfilelist
         {
             get { return backupfilelist; }
             set { backupfilelist = value; }
         }


    }
}
