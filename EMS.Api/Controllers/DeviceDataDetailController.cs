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

        [HttpGet("GetDeviceDataDetail/{deviceid}")]
        public async Task<IActionResult> GetDeviceDataDetail(int deviceid)
        {
            ResponseModel resp = new ResponseModel();
            try
            {
                resp.Data = await _dataDetailRrepository.GetDeviceDataDetailsAsync(deviceid);
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

        [HttpGet("GetDeviceStatus")]
        public async Task<IActionResult> GetDeviceStatus()
        {
            ResponseModel resp = new ResponseModel();
            try
            {
                resp.Data = await _dataDetailRrepository.GetDeviceStatus();
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




        [HttpGet("Get/{id}")]
        public async Task<IActionResult> Get(int id)
        {
            ResponseModel resp = new ResponseModel();
            try
            {
                List<DeviceDataDetailDTO> deviceDataDetailsDTO = await _dataDetailRrepository.Get(id);
                if (deviceDataDetailsDTO != null && deviceDataDetailsDTO.Count > 0)
                {
                    resp.IsSuccess = true;
                    resp.Message = ConstantMessages.DataSuccessMessage;
                    resp.Data = deviceDataDetailsDTO;
                }
                else
                {
                    resp.IsSuccess = false;
                    resp.Message = ConstantMessages.ErrorMessage;
                    resp.Data = null;
                }
            }
            catch (Exception ex)
            {
                resp.IsSuccess = false;
                resp.Message = ConstantMessages.ErrorMessage;
            }
            return Ok(resp);
        }

        [HttpGet("GetEnergyConspDatau1")]
        public async Task<IActionResult> GetEnergyConspDatau1()
        {
            ResponseModel resp = new ResponseModel();
            try
            {
                List<DeviceDataDetailDTO> deviceDataDetailsDTO = await _dataDetailRrepository.GetEnergyConspDatau1();
                if (deviceDataDetailsDTO != null && deviceDataDetailsDTO.Count > 0)
                {
                    resp.IsSuccess = true;
                    resp.Message = ConstantMessages.DataSuccessMessage;
                    resp.Data = deviceDataDetailsDTO;
                }
                else
                {
                    resp.IsSuccess = false;
                    resp.Message = ConstantMessages.ErrorMessage;
                    resp.Data = null;
                }
            }
            catch (Exception ex)
            {
                resp.IsSuccess = false;
                resp.Message = ConstantMessages.ErrorMessage;
            }
            return Ok(resp);
        }

        [HttpGet("GetEnergyConspDatau2")]
        public async Task<IActionResult> GetEnergyConspDatau2()
        {
            ResponseModel resp = new ResponseModel();
            try
            {
                List<DeviceDataDetailDTO> deviceDataDetailsDTO = await _dataDetailRrepository.GetEnergyConspDatau2();
                if (deviceDataDetailsDTO != null && deviceDataDetailsDTO.Count > 0)
                {
                    resp.IsSuccess = true;
                    resp.Message = ConstantMessages.DataSuccessMessage;
                    resp.Data = deviceDataDetailsDTO;
                }
                else
                {
                    resp.IsSuccess = false;
                    resp.Message = ConstantMessages.ErrorMessage;
                    resp.Data = null;
                }
            }
            catch (Exception ex)
            {
                resp.IsSuccess = false;
                resp.Message = ConstantMessages.ErrorMessage;
            }
            return Ok(resp);
        }

        [HttpGet("GetEnergyConspDatau3")]
        public async Task<IActionResult> GetEnergyConspDatau3()
        {
            ResponseModel resp = new ResponseModel();
            try
            {
                List<DeviceDataDetailDTO> deviceDataDetailsDTO = await _dataDetailRrepository.GetEnergyConspDatau3();
                if (deviceDataDetailsDTO != null && deviceDataDetailsDTO.Count > 0)
                {
                    resp.IsSuccess = true;
                    resp.Message = ConstantMessages.DataSuccessMessage;
                    resp.Data = deviceDataDetailsDTO;
                }
                else
                {
                    resp.IsSuccess = false;
                    resp.Message = ConstantMessages.ErrorMessage;
                    resp.Data = null;
                }
            }
            catch (Exception ex)
            {
                resp.IsSuccess = false;
                resp.Message = ConstantMessages.ErrorMessage;
            }
            return Ok(resp);
        }

        [HttpGet("GetAlert")]
        public async Task<IActionResult> GetAlert()
        {
            ResponseModel resp = new ResponseModel();
            try
            {
                List<DeviceDataDetailDTO> deviceDataDetailsDTO = await _dataDetailRrepository.GetAlert();
                if (deviceDataDetailsDTO != null && deviceDataDetailsDTO.Count > 0)
                {
                    resp.IsSuccess = true;
                    resp.Message = ConstantMessages.DataSuccessMessage;
                    resp.Data = deviceDataDetailsDTO;
                }
                else
                {
                    resp.IsSuccess = false;
                    resp.Message = ConstantMessages.ErrorMessage;
                    resp.Data = null;
                }
            }
            catch (Exception ex)
            {
                resp.IsSuccess = false;
                resp.Message = ConstantMessages.ErrorMessage;
            }
            return Ok(resp);
        }


        [HttpGet("GetPowerLoadu3")]
        public async Task<IActionResult> GetPowerLoadu3()
        {
            ResponseModel resp = new ResponseModel();
            try
            {
                List<DeviceDataDetailDTO> deviceDataDetailsDTO = await _dataDetailRrepository.GetPowerLoadu3();
                if (deviceDataDetailsDTO != null && deviceDataDetailsDTO.Count > 0)
                {
                    resp.IsSuccess = true;
                    resp.Message = ConstantMessages.DataSuccessMessage;
                    resp.Data = deviceDataDetailsDTO;
                }
                else
                {
                    resp.IsSuccess = false;
                    resp.Message = ConstantMessages.ErrorMessage;
                    resp.Data = null;
                }
            }
            catch (Exception ex)
            {
                resp.IsSuccess = false;
                resp.Message = ConstantMessages.ErrorMessage;
            }
            return Ok(resp);
        }



        [HttpGet("GetPowerLoadu1")]
        public async Task<IActionResult> GetPowerLoadu1()
        {
            ResponseModel resp = new ResponseModel();
            try
            {
                List<DeviceDataDetailDTO> deviceDataDetailsDTO = await _dataDetailRrepository.GetPowerLoadu1();
                if (deviceDataDetailsDTO != null && deviceDataDetailsDTO.Count > 0)
                {
                    resp.IsSuccess = true;
                    resp.Message = ConstantMessages.DataSuccessMessage;
                    resp.Data = deviceDataDetailsDTO;
                }
                else
                {
                    resp.IsSuccess = false;
                    resp.Message = ConstantMessages.ErrorMessage;
                    resp.Data = null;
                }
            }
            catch (Exception ex)
            {
                resp.IsSuccess = false;
                resp.Message = ConstantMessages.ErrorMessage;
            }
            return Ok(resp);
        }

        [HttpGet("GetPowerLoadu2")]
        public async Task<IActionResult> GetPowerLoadu2()
        {
            ResponseModel resp = new ResponseModel();
            try
            {
                List<DeviceDataDetailDTO> deviceDataDetailsDTO = await _dataDetailRrepository.GetPowerLoadu2();
                if (deviceDataDetailsDTO != null && deviceDataDetailsDTO.Count > 0)
                {
                    resp.IsSuccess = true;
                    resp.Message = ConstantMessages.DataSuccessMessage;
                    resp.Data = deviceDataDetailsDTO;
                }
                else
                {
                    resp.IsSuccess = false;
                    resp.Message = ConstantMessages.ErrorMessage;
                    resp.Data = null;
                }
            }
            catch (Exception ex)
            {
                resp.IsSuccess = false;
                resp.Message = ConstantMessages.ErrorMessage;
            }
            return Ok(resp);
        }


        [HttpGet("GetHistoricDeviceDataDetail")]
        public async Task<IActionResult> GetHistoricDeviceDataDetail(DateTime startDate, DateTime endDate, string parameter)
        {
            ResponseModel resp = new ResponseModel();

            try
            {
                // Log incoming parameters
                Console.WriteLine($"Received Request - Start Date: {startDate}, End Date: {endDate}");

                // Convert to UTC if needed
                DateTime utcStartDate = startDate.ToUniversalTime();
                DateTime utcEndDate = endDate.ToUniversalTime();

                // Fetch data from database
                var data = await _dataDetailRrepository.GetHistoricDeviceDataDetailsAsync(utcStartDate, utcEndDate, parameter);

                // Log the number of records retrieved
                //  Console.WriteLine($"Records Found: {data.Count}");

                resp.Data = data;
                resp.Message = ConstantMessages.DataSuccessMessage;
                resp.IsSuccess = true;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error: {ex.Message}");
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

        [HttpGet("Getpowerloadtoday")]
        public async Task<IActionResult> Getpowerloadtoday()
        {
            var response = new ResponseModel();
            try
            {
                var data = await _dataDetailRrepository.Getpowerloadtoday();

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


        [HttpGet("GetPowerLoadTodayHourly")]
        public async Task<IActionResult> GetPowerLoadTodayHourly()
        {
            var response = new ResponseModel();
            try
            {
                var data = await _dataDetailRrepository.GetPowerLoadTodayHourly();

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


        [HttpGet("GetPowerConsumptionRealtime")]
        public async Task<IActionResult> GetPowerConsumptionRealtime()
        {
            var response = new ResponseModel();
            try
            {
                var data = await _dataDetailRrepository.GetPowerConsumptionRealtime();

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




        [HttpGet("Getpowerloadhourly")]
        public async Task<IActionResult> Getpowerloadhourly()
        {
            var response = new ResponseModel();
            try
            {
                var data = await _dataDetailRrepository.Getpowerloadhourly();

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



        [HttpGet("GetkW")]
        public async Task<IActionResult> GetkW()
        {
            var response = new ResponseModel();
            try
            {
                var data = await _dataDetailRrepository.GetkW();

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
