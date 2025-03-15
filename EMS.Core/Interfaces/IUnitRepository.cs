using EMS.Core.Models;

namespace EMS.Core.Interfaces
{
    public interface IUnitRepository
    {
        Task<List<UnitDTO>> Get(int companyId);
        Task<List<UnitDTO>> GetUnitsByProjectId(int projectId,int companyId);
        Task<UnitDTO> GetById(int id, int companyId);
        Task Insert(UnitDTO obj);
        Task Update(UnitDTO obj);
        Task Delete(int id);


    }
}
