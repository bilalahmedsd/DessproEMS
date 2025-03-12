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
        [HttpGet("GetfilterDeviceDataDetails")]
        public async Task<IActionResult> GetfilterDeviceDataDetails(
     [FromQuery] int? projectId,
     [FromQuery] int? meterId,
     [FromQuery] int? unitId,
     [FromQuery] DateTime startDate,
     [FromQuery] DateTime endDate,
     [FromQuery] string? timeRange)
        {
            ResponseModel resp = new ResponseModel();
            try
            {
                var data = await _dataDetailRrepository.GetfilterDeviceDataDetails(
                    projectId, meterId, unitId, startDate, endDate, timeRange);

                //if (data == null || !data.Any())
                //{
                //    resp.Message = "No data found for the given filters.";
                //    resp.IsSuccess = false;
                //    return NotFound(resp);
                //}

                resp.Data = data;
                resp.Message = ConstantMessages.DataSuccessMessage;
                resp.IsSuccess = true;

                return Ok(resp);
            }
            catch (Exception ex)
            {
                resp.Message = "An error occurred while fetching data.";
                resp.IsSuccess = false;
               

                return StatusCode(500, resp);
            }
        }

    }
}
