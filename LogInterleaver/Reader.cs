using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace LogInterleaver {
    class Reader {
        private readonly StreamReader _reader;
        private readonly Regex _regex;
        private readonly string _format;
        
        private string _currentLog;
        private DateTime _currentTime;
        
        private string _nextLine;
        
        private bool _disposed;
        
        public DateTime? Time {
            get {
                if (_disposed)
                    return null;

                return _currentTime;
            }
        }

        public Reader(string file, string timestampFormat, Regex timestampRegex) {
            _regex = timestampRegex;
            _format = timestampFormat;
            _reader = new StreamReader(File.OpenRead(file));
        }
        
        

        public DateTime? NextTimestamp() {
            if (_disposed) {
                return null;
            }

            if (_currentLog != null) {
                return _currentTime;
            }

            if (_nextLine == null) {
                _nextLine = _reader.ReadLine();
                if (_nextLine == null) {
                    _reader.Dispose();
                    _disposed = true;
                    _currentLog = null;
                    return null;
                }
            }

            
            var timestamp = _regex.Match(_nextLine).Value;
            if (!DateTime.TryParseExact(timestamp, _format, CultureInfo.InvariantCulture, DateTimeStyles.None,
                out _currentTime)) {
                throw new Exception($"Next line not contains timestamps!\n{_nextLine}\n");
            }
            
            var builder = new StringBuilder(_nextLine);
            
            while (true) {
                _nextLine = _reader.ReadLine();
                if (_nextLine != null && !_regex.Match(_nextLine).Success) {
                    builder.Append('\n').Append(_nextLine);
                    continue;
                }
                
                _currentLog = builder.ToString();
                return _currentTime;
            }
        }

        public string TakeCurrent() {
            if (_disposed) {
                return null;
            }
            
            var temp = _currentLog;
            _currentLog = null;
            return temp;
        }

        public void SkipCurrent() {
            _currentLog = null;
        }

        public string Current() {
            return _currentLog;
        }
        
    }
}