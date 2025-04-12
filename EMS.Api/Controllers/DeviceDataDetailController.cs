using EMS.Core.Helper;
using EMS.Core.Interfaces;
using EMS.Core.Models;
using EMS.Core.Services;
using EMS.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace EMS.Api.Controllers
{
    public class DeviceDataDetailController : BaseController
    {
        private readonly IDeviceDataDetailRepository _dataDetailRrepository;
        public DeviceDataDetailController(IDeviceDataDetailRepository deviceDataDetailRepository, IUserServices services) : base(services)
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

        // [HttpGet("GetfilterDeviceDataDetail")]
        // public async Task<IActionResult> GetfilterDeviceDataDetail(
        //[FromQuery] IEnumerable<int> projectId,
        //[FromQuery] IEnumerable<int> unitId,
        //[FromQuery] Dictionary<string, List<int>> meterId,
        //[FromQuery] DateTime startDate,
        //[FromQuery] DateTime endDate,
        //[FromQuery] string timeRange)
        // {
        //     var response = new ResponseModel();
        //     try
        //     {
        //         var parsedMeterId = meterId.ToDictionary(k => int.Parse(k.Key), v => v.Value);
        //         var data = await _dataDetailRrepository.GetFilteredDeviceDataDetail(
        //             projectId.ToList(), unitId.ToList(), parsedMeterId, startDate, endDate, timeRange);

        //         response.Data = data;
        //         response.Message = "Successfully fetched!";
        //         response.IsSuccess = true;
        //     }
        //     catch (Exception ex)
        //     {
        //         response.Message = $"Error: {ex.Message}";
        //         response.IsSuccess = false;
        //     }
        //     return Ok(response);
        // }
        // [HttpPost("GetfilterDeviceDataDetailv2")]


        [HttpPost("GetfilterDeviceDataDetailv2")]
        public async Task<IActionResult> GetfilterDeviceDataDetailv2([FromBody] ProjectDataRequest request)
        {
            var response = new ResponseModel();
            try
            {
                var data = await _dataDetailRrepository.GetFilteredDeviceDataDetail2(request);

                response.Data = data;
                response.Message = "Successfully fetched!";
                response.IsSuccess = true;
            }
            catch (Exception ex)
            {
                response.Message = $"Error: {ex.Message}";
                response.IsSuccess = false;
            }
            return Ok(response);

        }

    }
}
