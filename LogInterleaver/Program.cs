using System;
using System.Text.RegularExpressions;

namespace LogInterleaver {
    class Program {
        static void Main(string[] args) {
            Console.WriteLine("Hello World!");

            var options = Options.Parse(args);
            if (!options.IsValid) {
                Console.WriteLine(options.Error);
                Console.WriteLine();
                Console.WriteLine(options.Help);
                return;
            }
            
            new Interleaver().Interleave(options);
        }
    }
}