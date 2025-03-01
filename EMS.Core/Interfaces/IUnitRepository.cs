using EMS.Core.Models;

namespace EMS.Core.Interfaces
{
    public interface IUnitRepository
    {
        Task<List<UnitDTO>> Get();
        Task<UnitDTO> GetById(int id);
        Task Insert(UnitDTO obj);
        Task Update(UnitDTO obj);
        Task Delete(int id);


    }
}
