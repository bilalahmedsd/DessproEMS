using EMS.Core.Models;


namespace EMS.Core.Interfaces
{
    public interface IProjectManagementRepository
    {
        Task<List<ProjectManagementDTO>> Get(int companyID);
        Task<ProjectManagementDTO> Get(int id, int companyID);
        Task Insert(ProjectManagementDTO obj);
        Task Update(ProjectManagementDTO obj);
    }
}
