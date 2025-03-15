using EMS.Core.Helper;
using EMS.Core.Interfaces;
using EMS.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Reflection.Metadata;

namespace EMS.Api.Controllers
{
    [Authorize]
    public class ProjectManagementController : BaseController
    {
        private readonly IProjectManagementRepository _projectmanagementrepository;

        public ProjectManagementController(IProjectManagementRepository projectmanagementRepository)
        {
            _projectmanagementrepository = projectmanagementRepository;
        }
        [HttpGet("Get")]
        public async Task<IActionResult> Get()
        {
            ResponseModel resp = new ResponseModel();
            try
            {
                resp.Data = await _projectmanagementrepository.Get(CurrentUser.FkCompanyId.Value);
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
