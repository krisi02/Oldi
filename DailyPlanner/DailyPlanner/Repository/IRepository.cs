using DailyPlanner.Models;
using DailyPlanner.Models.Enum;
using DailyPlanner.Models.NetworkModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DailyPlanner.Repository
{
    public interface IRepository
    {
        Task<Response<UserModel>> LoginAsync(string username, string password);
        Task<Response<bool>> LogoutAsync(string userId);
        Task ForceUserLogout();
        Task<bool> IsUserLoggedAsync();

        Task<UserModel> GetUser();

        Task<Response<TruckDriverMatrix>> GetDefaultTruckDriverMatrix();

        Task<Response<CausalsList>> GetCausals(CausalTypeEnum filter);
        Task<Response<bool>> CreateVacation(VacationModel model);
        Task<Response<bool>> UpdateVacation(VacationModel model);
        Task<Response<bool>> RevokeVacation(VacationModel model);
        Task<Response<VacationsList>> GetVacationsList(DateTime startingDate, DateTime endingDate);
        Task<Response<PlanningsList>> GetPlanningsListForDay(DateTime day);
        Task<Response<List<DriverModel>>> GetAvailibleDriversForDay(DateTime day);
        Task<Response<PlanningsList>> GetDriverNotifications();
        Task<Response<WorkingHoursList>> GetWorkingHoursList(DateTime day);
        Task<Response<TruckDriverMatrix>> GetTruckDriverMatrixForPlanning(DateTime day);
        Task<Response<PlantsList>> GetPlantList();
        Task<Response<AnomalyList>> GetAnomalies();
        Task<Response<bool>> CreatePlanning(Planning model);
        Task<Response<bool>> UpdatePlanning(Planning model);
        Task<Response<bool>> RevokePlanning(PlanningRevoke model);
        Task<Response<bool>> CompleteAnomaly(AnomalyConfirmation model);
        Task<Response<bool>> TransferAnomaly(AnomalyTransfer model);
        Task<Response<bool>> UnlinkTruckDriver(TruckDriverUnlinkModel model);

        Task<Response<bool>> DriverPlanningConfirmation(DriverPlanningConfirmation model);

    }
}
