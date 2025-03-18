using EMS.Core.Helper;
using EMS.Core.Interfaces;
using EMS.Core.Models;
using EMS.Core.Services;
using EMS.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EMS.Api.Controllers
{
    [Authorize]
    public class GatewayController : BaseController
    {
        private readonly IGatewayRepository _gatewayRepository;
        public GatewayController(IGatewayRepository gatewayRepository, IUserServices services) : base(services)
        {
            _gatewayRepository = gatewayRepository;

        }

        [HttpGet("Get")]
        public async Task<IActionResult> Get()
        {
            ResponseModel resp = new ResponseModel();
            try
            {
                resp.Data = await _gatewayRepository.Get(userServices.GetUser().FkCompanyId.Value);
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
        [HttpGet("GetGatewaybyUnitId/{UnitId}")]
        public async Task<IActionResult> GetGatewaybyUnitId(int UnitId)
        {
            ResponseModel resp = new ResponseModel();
            try
            {
                resp.Data = await _gatewayRepository.GetGatewaybyUnitId(UnitId, userServices.GetUser().FkCompanyId.Value);
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
        public async Task<IActionResult> Insert([FromBody] GatewayDTO gateway)
        {
            ResponseModel resp = new ResponseModel();
            try
            {
                if (gateway == null)
                {
                    resp.Message = "Invalid data!";
                    resp.IsSuccess = false;
                    return BadRequest(resp);
                }
                gateway.CreatedAt = CurrentDateTime;
                gateway.CreatedBy = userServices.GetUser().Id.Value;
                await _gatewayRepository.Insert(gateway);
                resp.Message = "Unit added successfully!";
                resp.IsSuccess = true;
                resp.Data = gateway;
            }
            catch (Exception ex)
            {
                resp.Message = "An error occurred while inserting unit.";
                resp.IsSuccess = false;
            }

            return Ok(resp);
        }
        [HttpPut("Update")]
        public async Task<IActionResult> Update([FromBody] GatewayDTO gateway)
        {
            ResponseModel resp = new ResponseModel();
            try
            {
                if (gateway == null || gateway.Id == null)
                {
                    resp.Message = "Invalid data!";
                    resp.IsSuccess = false;
                    return BadRequest(resp);
                }
                gateway.UpdatedAt = CurrentDateTime;
                gateway.UpdatedBy = userServices.GetUser().Id.Value;
                await _gatewayRepository.Update(gateway);
                resp.Message = "Gateway updated successfully!";
                resp.IsSuccess = true;
                resp.Data = gateway;
            }
            catch (Exception ex)
            {
                resp.Message = "An error occurred while updating the gateway.";
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

                await _gatewayRepository.Delete(id);
                resp.Message = "Gateway updated successfully!";
                resp.IsSuccess = true;
            }
            catch (Exception ex)
            {
                resp.Message = "An error occurred while updating the gateway.";
                resp.IsSuccess = false;
            }

            return Ok(resp);
        }

    }
}
