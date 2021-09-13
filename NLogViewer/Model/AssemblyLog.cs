using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NLogViewer
{
    internal class AssemblyLog : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public string FileName { get; set; }
        public string Title => Path.GetFileNameWithoutExtension(FileName);
        private bool _isLogVisible;
        public bool IsLogVisible
        {
            get { return _isLogVisible; }
            set
            {
                _isLogVisible = value;
                var _ = ParseLogEntriesAsync();
                OnPropertyChanged();
            }
        }

        public bool IsParsingInProgress { get; set; }
        public List<LogEntry> LogEntries { get; set; }

        public delegate void ParsingChangededHandler(object sender, EventArgs e);
        public event ParsingChangededHandler ParsingChanged;

        private void RaiseParsingChangedEvent()
        {
            ParsingChanged?.Invoke(this, new EventArgs());
        }

        public AssemblyLog(string fileName, bool isLogVisible)
        {
            LogEntries = new List<LogEntry>();
            FileName = fileName;
            IsLogVisible = isLogVisible;
            IsParsingInProgress = false;
        }

        public void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private async Task ParseLogEntriesAsync()
        {
            if (LogEntries.Count > 0 || !IsLogVisible || !File.Exists(FileName))
            {
                return;
            }
            IsParsingInProgress = true;
            RaiseParsingChangedEvent();
            await Task.Run(() => { ParseLogEntries(); });
        }

        private void ParseLogEntries()
        {
            try
            {
                string content = string.Empty;
                using (StreamReader reader = new StreamReader(FileName))
                {
                    content = reader.ReadToEnd();
                }
                string expression = @"(?=[0-9]{4}[-][0-9]{2}[-][0-9]{2}.[0-9]{2}:[0-9]{2}:[0-9]{1,2}.[0-9]{1,4}.*\([a-zA-Z]*\):)";  //2021-09-06 11:54:41.3143 (Info):Logger created by 
                string[] entryStrings = Regex.Split(content, expression);

                foreach (var entryString in entryStrings)
                {
                    LogEntry entry = new LogEntry();
                    string dateTimeExpression = @"[0-9]{4}[-][0-9]{2}[-][0-9]{2}.[0-9]{2}:[0-9]{2}:[0-9]{1,2}.[0-9]{1,4}";
                    Match dateTimeMatch = Regex.Match(entryString, dateTimeExpression);
                    if (!dateTimeMatch.Success)
                    {
                        continue;
                    }
                    entry.TimeStamp = DateTime.Parse(dateTimeMatch.Value);
                    string levelExpression = @"\([a-zA-Z]*\)";
                    Match levelMatch = Regex.Match(entryString, levelExpression);
                    if (!levelMatch.Success)
                    {
                        continue;
                    }
                    LogLevel level;
                    if (!Enum.TryParse(levelMatch.Value.TrimStart('(').TrimEnd(')'), out level))
                    {
                        continue;
                    }
                    entry.Level = level;

                    string messageSplitter = @"[0-9]{4}[-][0-9]{2}[-][0-9]{2}.[0-9]{2}:[0-9]{2}:[0-9]{1,2}.[0-9]{1,4}.*\([a-zA-Z]*\):";
                    string[] parts = Regex.Split(entryString, messageSplitter);
                    if (parts.Length < 2)
                    {
                        continue;
                    }
                    entry.Message = parts[1];
                    LogEntries.Add(entry);
                }
            }
            catch (Exception ex) { }
            finally
            {
                IsParsingInProgress = false;
                RaiseParsingChangedEvent();
            }
        }
    }
}
