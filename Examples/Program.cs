using System;
using System.Threading.Tasks;

namespace PLCConnectFramework.Examples
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("PLC Connect Framework Examples");
            Console.WriteLine("==============================");
            Console.WriteLine();
            
            Console.WriteLine("Select an example to run:");
            Console.WriteLine("1. Siemens S7 Direct Connection");
            Console.WriteLine();
            Console.Write("Enter your choice (1): ");
            
            string choice = Console.ReadLine();
            
            switch (choice)
            {
                case "1":
                case "":
                    await SiemensS7DirectExample.RunExampleAsync();
                    break;
                    
                default:
                    Console.WriteLine("Invalid choice");
                    break;
            }
        }
    }
}
