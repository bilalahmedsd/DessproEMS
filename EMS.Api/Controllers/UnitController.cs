using EMS.Core.Helper;
using EMS.Core.Interfaces;
using EMS.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EMS.Api.Controllers
{
    
    public class UnitController : BaseController
    {
        private readonly IUnitRepository _unitRepository;
        private readonly IDeviceRawDataRepository _deviceRawDataRepository;
        public UnitController(IUnitRepository unitRepository, IDeviceRawDataRepository deviceRawDataRepository)
        {
            _unitRepository = unitRepository;
            _deviceRawDataRepository = deviceRawDataRepository;
        }

        [HttpGet("Get")]
        public async Task<IActionResult> Get()
        {
            ResponseModel resp = new ResponseModel();
            try
            {
                resp.Data = await _unitRepository.Get();
                resp.Message = ConstantMessages.DataSuccessMessage;
                resp.IsSuccess = true;
            }
            catch (Exception ex)
            {
                resp.Message = ConstantMessages.ErrorMessage;
                resp.IsSuccess = false;

            }

            return Ok(resp);
        }
        [HttpGet("GetLatestDeviceData")]
        public  IActionResult GetLatest()
        {
            ResponseModel resp = new ResponseModel();
            try
            {
                resp.Data =  _deviceRawDataRepository.GetLatest();
                resp.Message = ConstantMessages.DataSuccessMessage;
                resp.IsSuccess = true;
            }
            catch (Exception ex)
            {
                resp.Message = ConstantMessages.ErrorMessage;
                resp.IsSuccess = false;

            }

            return Ok(resp);
        }
    }
}
