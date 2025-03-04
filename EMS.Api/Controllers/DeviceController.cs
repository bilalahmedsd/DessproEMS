using EMS.Core.Helper;
using EMS.Core.Interfaces;
using EMS.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace EMS.Api.Controllers
{
    public class DeviceController : BaseController
    {
        private readonly IDeviceRepository _deviceRepository;
        public DeviceController(IDeviceRepository deviceRepository)
        {
            _deviceRepository = deviceRepository;
        }

        [HttpGet("Get")]
        public async Task<IActionResult> Get()
        {
            ResponseModel resp = new ResponseModel();
            try
            {
                resp.Data = await _deviceRepository.Get();
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
