using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu_replay_renderer_netcore.CLI
{
    public static class CLIUtils
    {
        public static bool AskFileDelete(bool yesFlag, string file)
        {
            if (File.Exists(file))
            {
                Console.WriteLine("Warning: \"" + file + "\" already exists!");
                if (AskYNQuestion(yesFlag, "Do you want to delete this file?"))
                {
                    File.Delete(file);
                    Console.WriteLine("Success: \"" + file + "\" has been deleted!");
                }
                else return false;
            }
            return true;
        }

        public static bool AskYNQuestion(bool yesFlag, string question)
        {
            if (yesFlag) return true;
            Console.WriteLine(question + " [Y/n]?");
            var key = Console.ReadKey(false);
            if (key.Key == ConsoleKey.Y) return true;
            Console.WriteLine("'No' option selected!");
            return false;
        }
    }
}
