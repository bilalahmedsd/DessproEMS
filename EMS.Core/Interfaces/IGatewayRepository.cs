using EMS.Core.Models;
using System;


namespace EMS.Core.Interfaces
{
    public interface IGatewayRepository
    {
        Task<List<GatewayDTO>> Get(int companyId);
        Task<GatewayDTO> Get(int id,int companyId);
        Task<List<GatewayDTO>> GetGatewaybyUnitId(int unitId,int companyId);
        Task<List<GatewayDTO>> GetGatewayWithoutUnitId(int companyId);

        Task<List<GatewayDTO>> GetGatewaybyMultipleUnitIds(List<int> unitIds, int companyId);

        Task Insert(GatewayDTO obj);
        Task Update(GatewayDTO obj);
        Task Delete(int id);
    }
}
