using EMS.Core.Helper;
using EMS.Core.Interfaces;
using EMS.Core.Models;
using EMS.Data.Models;
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
        [HttpPost("Insert")]
        public async Task<IActionResult> Insert([FromBody] UnitDTO unit)
        {
            ResponseModel resp = new ResponseModel();
            try
            {
                if (unit == null)
                {
                    resp.Message = "Invalid data!";
                    resp.IsSuccess = false;
                    return BadRequest(resp);
                }

                await _unitRepository.Insert(unit);
                resp.Message = "Unit added successfully!";
                resp.IsSuccess = true;
                resp.Data = unit;
            }
            catch (Exception ex)
            {
                resp.Message = "An error occurred while inserting unit.";
                resp.IsSuccess = false;
            }

            return Ok(resp);
        }
        [HttpPut("Update")]
        public async Task<IActionResult> Update([FromBody] UnitDTO unit)
        {
            ResponseModel resp = new ResponseModel();
            try
            {
                if (unit == null || unit.Id == null)
                {
                    resp.Message = "Invalid data!";
                    resp.IsSuccess = false;
                    return BadRequest(resp);
                }

                await _unitRepository.Update(unit);
                resp.Message = "Unit updated successfully!";
                resp.IsSuccess = true;
                resp.Data = unit;
            }
            catch (Exception ex)
            {
                resp.Message = "An error occurred while updating the unit.";
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

                await _unitRepository.Delete(id);
                resp.Message = "Unit updated successfully!";
                resp.IsSuccess = true;
            }
            catch (Exception ex)
            {
                resp.Message = "An error occurred while updating the unit.";
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
