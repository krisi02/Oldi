using DailyPlanner.Utility;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace DailyPlanner.ViewModels
{
    public class VacationReportingFilterDialogViewModel : BindableBase, IDialogAware
    {
        public VacationReportingFilterDialogViewModel()
        {
            BindCommands();
        }

        private void BindCommands()
        {
            SaveFilter = new DelegateCommand(() =>
            {
                var param = new DialogParameters();
                param.Add(Constants.SELECTED_STARTING_DATE_FILTER_KEY, StartingDate);
                param.Add(Constants.SELECTED_ENDING_DATE_FILTER_KEY, EndingDate);
                RequestClose(param);
                
            });
        }

        public event Action<IDialogParameters> RequestClose;

        public bool CanCloseDialog()
        {
            return true;
        }

        public void OnDialogClosed()
        {

        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
            StartingDate = parameters.GetValue<DateTime>(Constants.SELECTED_STARTING_DATE_FILTER_KEY);
            EndingDate = parameters.GetValue<DateTime>(Constants.SELECTED_ENDING_DATE_FILTER_KEY);
            RaisePropertyChanged(nameof(StartingDate));
            RaisePropertyChanged(nameof(EndingDate));
        }
        public ICommand SaveFilter { get; set; }
        public DateTime StartingDate { get; set; }
        public DateTime EndingDate { get; set; }
    }
}
