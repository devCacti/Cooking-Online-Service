using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace Cooking_Service.Logging
{
    /// <summary>
    /// DEPRECATED
    /// </summary>

    // The Logging class is a way of making debugging easier with custom logs made by the developer
    public class Logger
    {
        // File Exists Exception Prevention
        public static void FEEP()
        {
            // Create the directory if it doesn't exist
            if (!Directory.Exists("C:\\Cooking_Logs\\Operations\\"))
                Directory.CreateDirectory("C:\\Cooking_Logs\\Operations\\");

            // Create the log file if it doesn't exist
            if (!File.Exists("C:\\Cooking_Logs\\Operations\\CookingServiceLog.txt"))
                File.Create("C:\\Cooking_Logs\\Operations\\CookingServiceLog.txt").Close();
        }

        public static void Log(string message)
        {
            // File Exists Exception Prevention
            FEEP();
            string path = "C:\\Cooking_Logs\\Operations\\CookingServiceLog.txt";
            // Log message to a file
            using (StreamWriter writer = new StreamWriter(path, true))
            {
                writer.WriteLine("\n" + DateTime.Now.ToString() + ": " + message);
            }
        }

        public override string ToString()
        {
            // File Exists Exception Prevention
            FEEP();
            string path = "C:\\Cooking_Logs\\Operations\\CookingServiceLog.txt";
            // Read the log file
            using (StreamReader reader = new StreamReader(path))
            {
                return reader.ReadToEnd();
            }
        }
    }
}