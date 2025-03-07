using EMS.Core.Helper;
using EMS.Core.Interfaces;
using EMS.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace EMS.Api.Controllers
{
    public class DeviceDataDetailController : BaseController
    {
        private readonly IDeviceDataDetailRepository _dataDetailRrepository;
        public DeviceDataDetailController(IDeviceDataDetailRepository deviceDataDetailRepository)
        {
            _dataDetailRrepository = deviceDataDetailRepository;
        }
        [HttpGet("GetDeviceDataDetail")]
        public async Task<IActionResult> GetDeviceDataDetail()
        {
            ResponseModel resp = new ResponseModel();
            try
            {
                resp.Data = await _dataDetailRrepository.GetDeviceDataDetailsAsync();
                resp.Message = ConstantMessages.DataSuccessMessage;
                resp.IsSuccess = true;
            }
            catch
            {
                resp.Message = ConstantMessages.ErrorMessage;
                resp.IsSuccess = false;
            }
            return Ok(resp);

        }
        [HttpGet("GetHistoricDeviceDataDetail")]
        public async Task<IActionResult> GetHistoricDeviceDataDetail(DateTime startDate, DateTime endDate)
        {
            ResponseModel resp = new ResponseModel();
            try
            {
                resp.Data = await _dataDetailRrepository.GetHistoricDeviceDataDetailsAsync(startDate, endDate);
                resp.Message = ConstantMessages.DataSuccessMessage;
                resp.IsSuccess = true;
            }
            catch
            {
                resp.Message = ConstantMessages.ErrorMessage;
                resp.IsSuccess = false;
            }
            return Ok(resp);

        }
    }
}
