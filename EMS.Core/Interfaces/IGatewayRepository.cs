using EMS.Core.Models;
using System;


namespace EMS.Core.Interfaces
{
    public interface IGatewayRepository
    {
        Task<List<GatewayDTO>> Get();
        Task<GatewayDTO> Get(int id);
        Task Insert(GatewayDTO obj);
        Task Update(GatewayDTO obj);
        Task Delete(int id);
    }
}
