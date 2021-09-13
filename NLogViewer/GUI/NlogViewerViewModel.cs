using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;

namespace NLogViewer
{
    internal class NlogViewerViewModel : INotifyPropertyChanged
    {
        public int SLIDER_RESOLUTION => int.MaxValue;
        public SimpleCommand LoadCommand { get; private set; }
        public SimpleCommand ForwardCommand { get; private set; }
        public SimpleCommand ForwardMinuteCommand { get; private set; }
        public SimpleCommand BackwardCommand { get; private set; }
        public SimpleCommand BackwardMinuteCommand { get; private set; }
        public ObservableCollection<AssemblyLog> FilterableComponentsLog { get; private set; }
        public ObservableCollection<LogEntry> VisibleComponentsLog { get; private set; }


        private bool _isAllSelected;

        public event PropertyChangedEventHandler PropertyChanged;

        private DateTime _startingDate;
        private DateTime _endingDate;

        private DateTime _selectedDate;
        public DateTime SelectedDate
        {
            get { return _selectedDate; }
            set
            {
                _selectedDate = value;
                OnPropertyChanged();
            }
        }


        public bool IsAllSelected
        {
            get { return _isAllSelected; }
            set
            {
                _isAllSelected = value;
                SelectAll();
                OnPropertyChanged();
            }
        }


        public bool IsLoading
        {
            get { return FilterableComponentsLog.Any(_ => _.IsParsingInProgress); }
            set
            {
                OnPropertyChanged();
                if (!value)
                {
                    UpdateVisibleLogs();
                }
            }
        }

        private double _sliderSelection;
        public double SliderSelection
        {
            get { return _sliderSelection; }
            set
            {
                _sliderSelection = value;
                UpdateVisibleLogs();
                OnPropertyChanged();
            }
        }


        public NlogViewerViewModel()
        {
            FilterableComponentsLog = new ObservableCollection<AssemblyLog>();
            VisibleComponentsLog = new ObservableCollection<LogEntry>();
            LoadCommand = new SimpleCommand(LoadLogs);
            ForwardCommand = new SimpleCommand(Forward);
            ForwardMinuteCommand = new SimpleCommand(ForwardMinute);
            BackwardCommand = new SimpleCommand(Backward);
            BackwardMinuteCommand = new SimpleCommand(BackwardMinute);
        }



        private void LoadLogs()
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.InitialDirectory = "C:\\";
            dialog.IsFolderPicker = true;
            if (dialog.ShowDialog() != CommonFileDialogResult.Ok)
            {
                return;
            }

            foreach (var oldLogs in FilterableComponentsLog)
            {
                oldLogs.ParsingChanged -= ComponentLog_ParsingChanged;
                oldLogs.PropertyChanged -= ComponentLog_PropertyChanged;
                oldLogs.LogEntries.Clear();
                oldLogs.LogEntries = null;
            }
            FilterableComponentsLog.Clear();
            VisibleComponentsLog.Clear();
            IsAllSelected = false;

            string selectedDirectory = dialog.FileName;
            string[] availableLogFiles = Directory.GetFiles(selectedDirectory, "*.log");
            availableLogFiles = availableLogFiles.Where(_ => !_.Contains("ERROR.log")).ToArray();
            foreach (var logFile in availableLogFiles)
            {
                bool isLogVisible = false;
                AssemblyLog componentLog = new AssemblyLog(logFile, isLogVisible);
                componentLog.ParsingChanged += ComponentLog_ParsingChanged;
                componentLog.PropertyChanged += ComponentLog_PropertyChanged;
                FilterableComponentsLog.Add(componentLog);
            }
        }

        private void ComponentLog_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var isloading = IsLoading;
            if (e.PropertyName == "IsLogVisible" && !isloading)
            {
                UpdateVisibleLogs();
            }
        }

        private void ComponentLog_ParsingChanged(object sender, EventArgs e)
        {
            if (!IsLoading)
            {
                UpdateVisibleLogs();
                IsLoading = false;
            }
            else
            {
                IsLoading = true;
            }
        }

        private void SelectAll()
        {
            foreach (var logComponent in FilterableComponentsLog)
            {
                logComponent.IsLogVisible = IsAllSelected;
            }
            //CheckLoadingState();
        }

        public void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private void UpdateVisibleLogs()
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                if (IsLoading)
                {
                    return;
                }

                VisibleComponentsLog.Clear();
                List<LogEntry> selectedLogComponents = new List<LogEntry>();
                foreach (var log in FilterableComponentsLog)
                {
                    if (!log.IsLogVisible)
                    {
                        continue;
                    }
                    foreach (var entry in log.LogEntries)
                    {
                        selectedLogComponents.Add(entry);
                    }
                }
                List<LogEntry> orderedLogs = selectedLogComponents.OrderBy(_ => _.TimeStamp).ToList();
                if (orderedLogs.Count < 1)
                {
                    return;
                }
                _startingDate = orderedLogs.First().TimeStamp;
                _endingDate = orderedLogs.Last().TimeStamp;
                SelectedDate = GetSelectedDate();
                TimeSpan oneSecond = TimeSpan.FromSeconds(1);
                List<LogEntry> selectedLogs = orderedLogs.Where(_ => Math.Abs((SelectedDate - _.TimeStamp).TotalSeconds) <= 0.5).ToList();
                foreach (var log in selectedLogs)
                {
                    VisibleComponentsLog.Add(log);
                }
            }));
        }

        private DateTime GetSelectedDate()
        {
            TimeSpan span = _endingDate - _startingDate;
            TimeSpan step = new TimeSpan(span.Ticks / SLIDER_RESOLUTION);
            TimeSpan duration = TimeSpan.FromTicks(step.Ticks * (int)Math.Floor(SliderSelection));
            DateTime result = _startingDate + duration;
            return result;
        }

        private void Forward()
        {
            if (SliderSelection == SLIDER_RESOLUTION)
            {
                return;
            }
            do
            {
                SliderSelection += GetOneSecondSliderDifference();
            } while (VisibleComponentsLog.Count < 1);

        }

        private void ForwardMinute()
        {
            if (SliderSelection == SLIDER_RESOLUTION)
            {
                return;
            }
            SliderSelection += GetOneMinuteSliderDifference();

        }
        private void Backward()
        {
            if (SliderSelection == 0)
            {
                return;
            }
            do
            {
                SliderSelection -= GetOneSecondSliderDifference();
            } while (VisibleComponentsLog.Count < 1);
        }

        private void BackwardMinute()
        {
            if (SliderSelection == 0)
            {
                return;
            }
            SliderSelection -= GetOneSecondSliderDifference();
        }

        private int GetOneSecondSliderDifference()
        {
            TimeSpan totalSpan = _endingDate - _startingDate;
            int x = (int)Math.Ceiling(SLIDER_RESOLUTION / totalSpan.TotalSeconds);
            return x;
        }
        private int GetOneMinuteSliderDifference()
        {
            TimeSpan totalSpan = _endingDate - _startingDate;
            int x = (int)Math.Ceiling(SLIDER_RESOLUTION / totalSpan.TotalMinutes);
            return x;
        }
    }
}
