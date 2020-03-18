using System;
using System.IO;
using System.Linq;

namespace LogInterleaver {
    class Interleaver {

        public void Interleave(Options options) {
            var readers = options.Files
                .Select(x => new Reader(x, options.TimestampFormat, options.TimestampRegex))
                .ToList();


            var needToCut = options.EndtTime > default(DateTime);
            var needToSkip = options.StartTime > default(DateTime);
            var comparison = options.CaseSensetive
                ? StringComparison.InvariantCulture
                : StringComparison.InvariantCultureIgnoreCase;
            
            long index = 0;

            if (!string.IsNullOrWhiteSpace(options.SearchString)) {
                Console.WriteLine($"Search string: '{options.SearchString}'");
                Console.WriteLine($"Case {(options.CaseSensetive ? "sensetive" : "insensetive")}");
            }

            if (options.SearchRegex != null) {
                Console.WriteLine($"Search regex: '{options.SearchRegex}'");
            }
            
            if (needToSkip) {
                Console.Write($"Skipping to {options.StartTime.Value.ToString(options.TimestampFormat)}..      ");
                index = 1;
            }
            
            using (var writer = new StreamWriter(File.Open(options.OutputPath, FileMode.Create))) {
                while (true) {
                    var reader = readers
                        .Where(x => x.NextTimestamp() != null)
                        .OrderBy(x => x.NextTimestamp())
                        .FirstOrDefault();
                    
                    if (reader == null || (needToCut && options.EndtTime < reader.Time)) {
                        writer.Flush();
                        Console.CursorLeft = 0;
                        Console.Write(new string(' ', 80));
                        Console.CursorLeft = 0;
                        Console.WriteLine("\nALL DONE!");
                        return;
                    }

                    if (options.StartTime > reader.Time) {
                        reader.SkipCurrent();
                        Console.CursorLeft = Console.CursorLeft - 5;
                        Console.Write($"{index++:00000}");
                        continue;
                    }
                    
                    if (index > 0) {
                        Console.WriteLine();
                        index = 0;
                    }

                    if (!string.IsNullOrWhiteSpace(options.SearchString)) {
                        if (!reader.Current().Contains(options.SearchString, comparison)) {
                            reader.SkipCurrent();
                            continue;
                        }
                    }
                    
                    writer.WriteLine(reader.TakeCurrent());
                    Console.CursorLeft = 0;
                    Console.Write(reader.Time?.ToString(options.TimestampFormat));
                }
            }
        }
    }
}