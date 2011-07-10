using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AbstractionDependency
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("");
            Console.WriteLine("Running application:");
            var runningApplication = new Application();
            runningApplication.Run();

            Console.WriteLine("");
            Console.WriteLine("Test application using abstraction:");
            new ApplicationTest1().TestRun();

            Console.WriteLine("");
            Console.WriteLine("Test application using in memory listener:");
            new ApplicationTest2().TestRun();

            Console.ReadLine();
        }
    }
}
