using EMS.Core.Models;


namespace EMS.Core.Interfaces
{
    public interface IProjectManagementRepository
    {
        Task<List<ProjectManagementDTO>> Get();
        Task<ProjectManagementDTO> Get(int id);
        Task Insert(ProjectManagementDTO obj);
        Task Update(ProjectManagementDTO obj);
    }
}
