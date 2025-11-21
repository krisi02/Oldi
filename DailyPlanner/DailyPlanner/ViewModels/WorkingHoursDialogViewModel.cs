using DailyPlanner.Utility;
using DailyPlanner.ViewModels.Cards;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace DailyPlanner.ViewModels
{
    public class WorkingHoursDialogViewModel : BindableBase, IDialogAware
    {

        public WorkingHoursDialogViewModel()
        {
            BindCommands();
        }

        public event Action<IDialogParameters> RequestClose;

        private void BindCommands()
        {
            CloseDialog = new DelegateCommand(() =>
            {
                RequestClose(null);
            });
        }

        public bool CanCloseDialog()
        {
            return true;
        }

        public void OnDialogClosed()
        {

        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
            workingHoursModel = parameters.GetValue<WorkingHoursCardViewModel>(Constants.SELECTED_WH_CARD_KEY);
            RaisePropertyChanged(nameof(DriverName));
            RaisePropertyChanged(nameof(TruckTypeColor));
            RaisePropertyChanged(nameof(TruckInternalNumber));
            RaisePropertyChanged(nameof(TruckTypeDescription));
            RaisePropertyChanged(nameof(ContractType));

            RaisePropertyChanged(nameof(OfficialTotalHours));
            RaisePropertyChanged(nameof(OfficialHoursDescription));

            RaisePropertyChanged(nameof(PlannedTotalHours));
            RaisePropertyChanged(nameof(PlannedHoursDescription));

            RaisePropertyChanged(nameof(DriverTotalHours));
            RaisePropertyChanged(nameof(DriverHoursDescription));

            RaisePropertyChanged(nameof(TruckmoveTotalHours));
            RaisePropertyChanged(nameof(TruckmoveHoursDescription));

            RaisePropertyChanged(nameof(PausesTotalHours));
            RaisePropertyChanged(nameof(IsMessageVisible));
        }
        #region HEADER
        private WorkingHoursCardViewModel workingHoursModel { get; set; }
        public string DriverName { get { return workingHoursModel == null ? "" : workingHoursModel.DriverName; } }
        public string TruckInternalNumber { get { return workingHoursModel == null ? "" : workingHoursModel.TruckInternalNumber; } }
        public string TruckTypeDescription { get { return workingHoursModel == null ? "" : workingHoursModel.TruckTypeDescription; } }
        public string TruckTypeColor { get { return workingHoursModel == null ? "" : workingHoursModel.TruckTypeColor; } }

        public string ContractType
        {
            get
            {
                if (workingHoursModel == null)
                {
                    return "";
                }
                else
                {
                    return workingHoursModel.workingHours.IsRental != 0 ? workingHoursModel.workingHours.RentalDescription : "MEZZO FISSO";
                }
            }
        }
        #endregion

        #region OFFICIAL DATA
        public string OfficialTotalHours { get { return workingHoursModel == null ? "" : workingHoursModel.workingHours.DailyTimeDurationSTR + " ORE"; } }
        public string OfficialHoursDescription
        {
            get
            {
                return "DALLE " + OfficialStartingTime + " ALLE " + OfficialEndingTime;
            }
        }
        private string OfficialStartingTime
        {
            get
            {
                if (workingHoursModel == null)
                {
                    return "";
                }
                if (workingHoursModel.workingHours.DP_StartingTime.Date == new DateTime(1980, 1, 1).Date)
                {
                    return "N.D";
                }
                else
                {
                    return workingHoursModel.workingHours.DP_StartingTime.ToString("HH:mm");
                }
            }
        }
        private string OfficialEndingTime
        {
            get
            {
                if (workingHoursModel == null)
                {
                    return "";
                }
                if (workingHoursModel.workingHours.LV_EndingTime.Date == new DateTime(1980, 1, 1).Date)
                {
                    return "N.D";
                }
                else
                {
                    return workingHoursModel.workingHours.LV_EndingTime.ToString("HH:mm");
                }
            }
        }
        #endregion

        #region PLANNED DATA
        public string PlannedTotalHours { get { return workingHoursModel == null ? "" : workingHoursModel.workingHours.DP_DailyTimeDurationSTR + " ORE"; } }
        public string PlannedHoursDescription
        {
            get
            {
                return "DALLE " + PlannedStartingTime + " ALLE " + PlannedEndingTime;
            }
        }
        private string PlannedStartingTime
        {
            get
            {
                if (workingHoursModel == null)
                {
                    return "";
                }
                if (workingHoursModel.workingHours.DP_StartingTime.Date == new DateTime(1980, 1, 1).Date)
                {
                    return "N.D";
                }
                else
                {
                    return workingHoursModel.workingHours.DP_StartingTime.ToString("HH:mm");
                }
            }
        }
        private string PlannedEndingTime
        {
            get
            {
                if (workingHoursModel == null)
                {
                    return "";
                }
                if (workingHoursModel.workingHours.DP_EndingTime.Date == new DateTime(1980, 1, 1).Date)
                {
                    return "N.D";
                }
                else
                {
                    return workingHoursModel.workingHours.DP_EndingTime.ToString("HH:mm");
                }
            }
        }
        #endregion

        #region DRIVER DATA
        public string DriverTotalHours { get { return workingHoursModel == null ? "" : workingHoursModel.workingHours.TM_DriverDailyTimeDurationSTR + " ORE"; } }
        public string DriverHoursDescription
        {
            get
            {
                return "DALLE " + DriverStartingTime + " ALLE " + DriverEndingTime;
            }
        }
        private string DriverStartingTime
        {
            get
            {
                if (workingHoursModel == null)
                {
                    return "";
                }
                if (workingHoursModel.workingHours.TM_StartingTimeDriver.Date == new DateTime(1980, 1, 1).Date)
                {
                    return "N.D";
                }
                else
                {
                    return workingHoursModel.workingHours.TM_StartingTimeDriver.ToString("HH:mm");
                }
            }
        }
        private string DriverEndingTime
        {
            get
            {
                if (workingHoursModel == null)
                {
                    return "";
                }
                if (workingHoursModel.workingHours.TM_EndingTimeDriver.Date == new DateTime(1980, 1, 1).Date)
                {
                    return "N.D";
                }
                else
                {
                    return workingHoursModel.workingHours.TM_EndingTimeDriver.ToString("HH:mm");
                }
            }
        }

        #endregion

        #region TRUCKMOVE DATA
        public string TruckmoveTotalHours { get { return workingHoursModel == null ? "" : workingHoursModel.workingHours.TM_AutoDailyTimeDurationSTR + " ORE"; } }
        public string TruckmoveHoursDescription
        {
            get
            {
                return "DALLE " + TruckmoveStartingTime + " ALLE " + TruckmoveEndingTime;
            }
        }
        private string TruckmoveStartingTime
        {
            get
            {
                if (workingHoursModel == null)
                {
                    return "";
                }
                if (workingHoursModel.workingHours.TM_StartingTimeAuto.Date == new DateTime(1980, 1, 1).Date)
                {
                    return "N.D";
                }
                else
                {
                    return workingHoursModel.workingHours.TM_StartingTimeAuto.ToString("HH:mm");
                }
            }
        }
        private string TruckmoveEndingTime
        {
            get
            {
                if (workingHoursModel == null)
                {
                    return "";
                }
                if (workingHoursModel.workingHours.TM_EndingTimeAuto.Date == new DateTime(1980, 1, 1).Date)
                {
                    return "N.D";
                }
                else
                {
                    return workingHoursModel.workingHours.TM_EndingTimeAuto.ToString("HH:mm");
                }
            }
        }

        #endregion

        #region PAUSES DATA
        public string PausesTotalHours { 
            get 
            {
                if(workingHoursModel == null)
                {
                    return "";
                }

                return workingHoursModel.workingHours.BreakTimeDuration != 0 ? workingHoursModel.workingHours.BreakTimeDurationSTR + " ORE" : "NESSUNA PAUSA RILEVATA";
            } 
        }
        #endregion

        public bool IsMessageVisible { get
            {
                if (workingHoursModel == null)
                {
                    return false;
                }
                return workingHoursModel.workingHours.DailyStatus.Split('-')[0] == "R" ? true : false;
            } 
        }
        public ICommand CloseDialog { get; set; }

    }
}