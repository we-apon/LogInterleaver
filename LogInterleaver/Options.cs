using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace LogInterleaver {
    class Options {
        
        public List<string> Files { get; } = new List<string>();

        public string OutputPath { get; private set; }

        public Regex TimestampRegex { get; private set; }
        
        public string TimestampFormat { get; private set; }

        
        public string SearchString { get; private set; }
        
        public Regex SearchRegex { get; private set; }

        public bool CaseSensetive { get; private set; }

        
        private string StartTimeString { get; set; }
        private DateTime? _startTime;
        public DateTime? StartTime => getTime(ref _startTime, StartTimeString);
        
        
        private string EndTimeString { get; set; }
        private DateTime? _endTime;
        public DateTime? EndtTime => getTime(ref _endTime, EndTimeString);

        
        private DateTime? getTime(ref DateTime? _timeField, string timeString) {
            if (_timeField != null)
                return _timeField.Value;

            if (string.IsNullOrWhiteSpace(timeString)) {
                _timeField = new DateTime();
                return _timeField.Value;
            }

            if (!string.IsNullOrWhiteSpace(TimestampFormat)) {
                _timeField = DateTime.ParseExact(timeString, TimestampFormat, CultureInfo.InvariantCulture,
                    DateTimeStyles.None);
                return _timeField.Value;
            }

            if (DateTime.TryParse(timeString, out var date)) {
                _timeField = date;
                return date;
            }

            return null;
        }






        public string Error { get; private set; }
        
        public bool IsValid {
            get => string.IsNullOrWhiteSpace(Error);
        }


        public string Help {
            get => "Need Help!";
        }


        static Options Default() {
            var options = new Options {
                //TimestampRegex = new Regex(@"\d{4}\\\d{2}-\d{2} \d{2}:\d{2}:\d{2},\d{3}"),
                //TimestampFormat = "yyyy\\MM-dd HH:mm:ss:zzz",
                //OutputPath = Path.GetFullPath("result.log")
            };
            return options;
        }
        
        
        public static Options Parse(string[] args) {

            var options = Default();

            var regexSpecified = false;
            
            for (int i = 0; i < args.Length; i++) {
                switch (args[i]) {
                    case "-h":
                    case "--help":
                        return options;

                    case "--tr":
                    case "--timeregex":
                        if (args.Length > ++i && !string.IsNullOrWhiteSpace(args[i])) {
                            options.TimestampRegex = new Regex(args[i]);
                            regexSpecified = true;
                            continue;
                        }

                        options.Error = "can't read timestamp regex from command line!";
                        return options;
                    
                    case "--tf":
                    case "--timeformat":
                        if (args.Length > ++i && !string.IsNullOrWhiteSpace(options.TimestampFormat = args[i])) {
                            if (regexSpecified) {
                                continue;
                            }

                            var builder = new StringBuilder();
                            foreach (var chr in options.TimestampFormat) {
                                switch (chr) {
                                    case 'y':
                                    case 'M':
                                    case 'd':
                                    case 'H':
                                    case 'm':
                                    case 's':
                                    case 'f':
                                        builder.Append("\\d");
                                        break;
                                    
                                    case '\\':
                                        builder.Append(@"\\");
                                        break;
                                    
                                    default:
                                        builder.Append(chr);                                        
                                        break;
                                }
                            }
                            
                            options.TimestampRegex = new Regex(builder.ToString());
                            continue;
                        }
                        options.Error = "can't read timestamp format from command line";
                        return options;
                    
                    case "--ts":
                    case "--timestart":
                        if (args.Length > ++i && !string.IsNullOrWhiteSpace(options.StartTimeString = args[i])) {
                            continue;
                        }
                        
                        options.Error = "can't read start time from command line";
                        return options;

                    case "--te":
                    case "--timeend":
                        if (args.Length > ++i && !string.IsNullOrWhiteSpace(options.EndTimeString = args[i])) {
                            continue;
                        }
                        
                        options.Error = "can't read end time from command line";
                        return options;
                    
                    case "-s":
                    case "--search":
                        if (args.Length > ++i && !string.IsNullOrWhiteSpace(options.SearchString = args[i])) {
                            continue;
                        }
                        
                        options.Error = "can't read search string from command line";
                        return options;
                    
                    case "--sx":
                    case "--searchregex":
                        if (args.Length > ++i && !string.IsNullOrWhiteSpace(args[i])) {
                            options.SearchRegex = new Regex(args[i]);
                            continue;
                        }
                        
                        options.Error = "can't read search regex from command line";
                        return options;
                    
                    case "--cs":
                    case "--casesensetive":
                        options.CaseSensetive = true;
                        continue;
                    
                    case "-o":
                    case "--output":
                        if (args.Length > ++i && !string.IsNullOrWhiteSpace(options.OutputPath = args[i])) {
                            options.OutputPath = Path.GetFullPath(options.OutputPath);
                            continue;
                        }

                        options.Error = "can't find output path";
                        return options;

                    default:
                        var file = args[i].Trim();
                        if (string.IsNullOrWhiteSpace(file)) {
                            continue;
                        }

                        file = Path.GetFullPath(file);
                        if (!File.Exists(file)) {
                            options.Error = $"input file '{file}' doesn't exists!";
                            return options;
                        }
                        
                        options.Files.Add(file);
                        break;
                }
            }

            if (!options.Files.Any()) {
                options.Error = "input files not specified!";
                return options;
            }

            if (string.IsNullOrWhiteSpace(options.OutputPath)) {
                options.Error = "output file not specified!";
            }

            if (string.IsNullOrWhiteSpace(options.TimestampRegex?.ToString())) {
                options.Error = "timestamp regex not specified!";
            }

            if (string.IsNullOrWhiteSpace(options.TimestampFormat)) {
                options.Error = "timestamp format not specified!";
            }

            if (!string.IsNullOrWhiteSpace(options.StartTimeString) && options.StartTime == null) {
                options.Error = "start time not recognized!";
            }

            if (!string.IsNullOrWhiteSpace(options.EndTimeString) && options.EndtTime == null) {
                options.Error = "end time not recognized!";
            }

            return options;
        }


    }
}