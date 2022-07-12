﻿using Newtonsoft.Json;
using NHM.Common;
using NHMCore.Configs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NHMCore.Schedules
{
    public class SchedulesManager : NotifyChangedBase
    {
        public static SchedulesManager Instance { get; } = new SchedulesManager();
        private static readonly object _lock = new object();

        private SchedulesManager()
        { }

        private ObservableCollection<Schedule> _schedules = new ObservableCollection<Schedule>();

        public ObservableCollection<Schedule> Schedules
        {
            get
            {
                lock (_lock)
                {
                    return _schedules;
                }
            }
        }

        public void Init()
        {
            if (File.Exists(Paths.ConfigsPath("Schedule.json"))){
                _schedules = JsonConvert.DeserializeObject<ObservableCollection<Schedule>>(File.ReadAllText(Paths.ConfigsPath("Schedule.json")));
                OnPropertyChanged(nameof(Schedules));
            }
        }

        public void AddScheduleToList(Schedule schedule)
        {
            _schedules.Add(schedule);
            OnPropertyChanged(nameof(Schedules));
            ConfigManager.ScheduleConfigFileCommit();
        }

        public void DeleteScheduleFromList(Schedule schedule)
        {
            _schedules.Remove(schedule);
            OnPropertyChanged(nameof(Schedules));
            ConfigManager.ScheduleConfigFileCommit();
        }
    }
}
