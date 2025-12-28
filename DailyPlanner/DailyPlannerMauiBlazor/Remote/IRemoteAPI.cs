using DailyPlannerMauiBlazor.Models;
using DailyPlannerMauiBlazor.Models.NetworkModels;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace DailyPlannerMauiBlazor.Remote
{
    public interface IRemoteAPI
    {
        string ApiMethodPrefix { get; }
        HttpClient HttpClient { get; }
        Task<Response<UserModel>> LoginAsync(string loginName, string password, string fcmToken);
        Task<Response<bool>> LogoutAsync(string userId);
        Task<Response<TruckDriverMatrix>> GetTruckDriverDefaultMatrix();
        Task<Response<CausalsList>> GetRevocationCausals();
        Task<Response<CausalsList>> GetVacationCausals();
        Task<Response<bool>> CreateVacation(VacationModel model);
        Task<Response<bool>> UpdateVacation(VacationModel model);
        Task<Response<bool>> RevokeVacation(VacationModel model);
        Task<Response<VacationsList>> GetVacationsList(DateTime startingDate, DateTime endingDate);
        Task<Response<WorkingHoursList>> GetWorkingHoursForDay(DateTime day);

        Task<Response<PlanningsList>> GetPlanningsForDay(DateTime day);
        Task<Response<TruckDriverMatrix>> GetTruckDriverMatrixForPlanning(DateTime day);
        Task<Response<List<DriverModel>>> GetAvailibleDriversForDay(DateTime day);
        Task<Response<PlanningsList>> GetDriverNotifications();
        Task<Response<PlantsList>> GetPlantList();
        Task<Response<AnomalyList>> GetAnomalies();
        Task<Response<bool>> CompleteAnomaly(AnomalyConfirmation model);
        Task<Response<bool>> TransferAnomaly(AnomalyTransfer model);
        Task<Response<bool>> CreatePlanning(Planning model);
        Task<Response<bool>> UpdatePlanning(Planning model);
        Task<Response<bool>> RevokePlanning(PlanningRevoke model);
        Task<Response<bool>> UnlinkTruckDriver(TruckDriverUnlinkModel model);
        Task<Response<bool>> DriverPlanningConfirmation(DriverPlanningConfirmation model);
    }
}
