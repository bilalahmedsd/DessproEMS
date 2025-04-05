using EMS.Core.Helper;
using EMS.Core.Interfaces;
using EMS.Core.Models;
using EMS.Core.Services;
using EMS.Data.Models;
using EMS.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EMS.Api.Controllers
{
    [Authorize]
    public class DeviceController : BaseController
    {
        private readonly IDeviceRepository _deviceRepository;
        public DeviceController(IDeviceRepository deviceRepository, IUserServices services) : base(services)
        {
            _deviceRepository = deviceRepository;
        }

        [HttpGet("Get")]
        public async Task<IActionResult> Get()
        {
            ResponseModel resp = new ResponseModel();
            try
            {
                resp.Data = await _deviceRepository.Get(userServices.GetUser().FkCompanyId.Value);
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
        [HttpGet("GetDevicesbyGatewayId/{GatewayId}")]
        public async Task<IActionResult> GetDevicesWithGateways(int GatewayId)
        {
            ResponseModel resp = new ResponseModel();
            try
            {
                resp.Data = await _deviceRepository.GetDevicesWithGateways(GatewayId,userServices.GetUser().FkCompanyId.Value);
                resp.Message = ConstantMessages.DataSuccessMessage;
                resp.IsSuccess = true;
            }
            catch (Exception ex)
            {
                resp.Message = "Error fetching data.";
                resp.IsSuccess = false;
            }
            return Ok(resp);
        }

        [HttpGet("GetDevicesWithoutGatewayId")]
        public async Task<IActionResult> GetDevicesWithoutGatewayId()
        {
            ResponseModel resp = new ResponseModel();
            try
            {
                resp.Data = await _deviceRepository.GetDevicesWithoutGatewayId(userServices.GetUser().FkCompanyId.Value);
                resp.Message = ConstantMessages.DataSuccessMessage;
                resp.IsSuccess = true;
            }
            catch (Exception ex)
            {
                resp.Message = "Error fetching data.";
                resp.IsSuccess = false;
            }
            return Ok(resp);
        }

        [HttpGet("GetDevicesByMultipleGateways")]
        public async Task<IActionResult> GetDevicesByMultipleGateways(string gatewayIds)
        {
            ResponseModel resp = new ResponseModel();
            try
            {
                // ✅ Convert comma-separated string to List<int>
                List<int> gatewayIdList = gatewayIds.Split(',').Select(int.Parse).ToList();

                var companyId = userServices.GetUser().FkCompanyId.Value;
                resp.Data = await _deviceRepository.GetDevicesWithMultipleGateways(gatewayIdList, companyId); // Call updated repository method
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

        [HttpPost("Insert")]
        public async Task<IActionResult> Insert([FromBody] DeviceDTO device)
        {
            ResponseModel resp = new ResponseModel();
            try
            {
                if (device == null)
                {
                    resp.Message = "Invalid data!";
                    resp.IsSuccess = false;
                    return BadRequest(resp);
                }
                device.CreatedBy = userServices.GetUser().Id.Value;
                device.CreatedAt = CurrentDateTime;
                await _deviceRepository.Insert(device);
                resp.Message = "device added successfully!";
                resp.IsSuccess = true;
                resp.Data = device;
            }
            catch (Exception ex)
            {
                resp.Message = "An error occurred while inserting device.";
                resp.IsSuccess = false;
            }

            return Ok(resp);
        }
        [HttpPut("Update")]
        public async Task<IActionResult> Update([FromBody] DeviceDTO device)
        {
            ResponseModel resp = new ResponseModel();
            try
            {
                if (device == null || device.Id == null)
                {
                    resp.Message = "Invalid data!";
                    resp.IsSuccess = false;
                    return BadRequest(resp);
                }
                device.UpdatedBy = userServices.GetUser().Id.Value;
                device.UpdatedAt = CurrentDateTime;
                await _deviceRepository.Update(device);
                resp.Message = "Device updated successfully!";
                resp.IsSuccess = true;
                resp.Data = device;
            }
            catch (Exception ex)
            {
                resp.Message = "An error occurred while updating the device.";
                resp.IsSuccess = false;
            }

            return Ok(resp);
        }

        [HttpPut("Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            ResponseModel resp = new ResponseModel();
            try
            {
                if (id <= 0)
                {
                    resp.Message = "Invalid data!";
                    resp.IsSuccess = false;
                    return BadRequest(resp);
                }

                await _deviceRepository.Delete(id);
                resp.Message = "Device updated successfully!";
                resp.IsSuccess = true;
            }
            catch (Exception ex)
            {
                resp.Message = "An error occurred while updating the device.";
                resp.IsSuccess = false;
            }

            return Ok(resp);
        }

    }
}
