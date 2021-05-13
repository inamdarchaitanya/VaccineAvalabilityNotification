using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VAN
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    namespace Common
    {
        static class Logger
        {
            public static bool WriteLog(string strMessage)
            {
                try
                {
                    FileStream objFilestream = new FileStream("Log.txt", FileMode.Append, FileAccess.Write);
                    StreamWriter objStreamWriter = new StreamWriter((Stream)objFilestream);
                    objStreamWriter.WriteLine(strMessage);
                    objStreamWriter.Close();
                    objFilestream.Close();
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }
    }


}
