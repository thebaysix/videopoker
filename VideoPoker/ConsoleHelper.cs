using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VideoPoker
{
    internal static class ConsoleHelper
    {
        public static string HoldsToString(bool[] holds)
        {
            string s = string.Empty;
            if (holds[0])
                s += "1";
            if (holds[1])
                s += "2";
            if (holds[2])
                s += "3";
            if (holds[3])
                s += "4";
            if (holds[4])
                s += "5";

            return s;
        }

        public static void WriteLine(GameMode mode)
        {
            if (mode != GameMode.Simulation)
                Console.WriteLine();
        }

        public static void WriteLine(string str, GameMode mode, bool isScore = false)
        {
            if (mode != GameMode.Simulation || isScore)
                Console.WriteLine(str);
        }

        public static void Write(string str, GameMode mode)
        {
            if (mode != GameMode.Simulation)
                Console.Write(str);
        }

        public static int Read(GameMode mode)
        {
            if (mode != GameMode.Simulation)
                return Console.Read();

            throw new Exception("Should not Read from prompt in Simulation mode.");
        }
    }
}
