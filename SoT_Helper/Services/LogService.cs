using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoT_Helper.Services
{
    public class LogService
    {
        private static string fileName = "debug.log";

        public static void Log(string message)
        {
            Console.WriteLine(message);
            List<string> log = new List<string>();
            if (File.Exists(fileName))
            {
                log = File.ReadAllLines(fileName).ToList();
            }
            log.Insert(0,message);
            File.WriteAllLines(fileName, log);
        }
    }
}
