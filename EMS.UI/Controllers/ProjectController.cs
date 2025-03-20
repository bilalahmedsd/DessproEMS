using EMS.Core.Models;
using EMS.UI.CustomFilter;
using EMS.UI.Utilities;
using Microsoft.AspNetCore.Mvc;

namespace EMS.UI.Controllers
{
    [AuthorizationFilter("1")]
    public class ProjectController : BaseController
    {
        public async Task<IActionResult> Get()
        {

            ResponseModel resp = new ResponseModel();
            try
            {
                resp = await ApiUtility.GetApi(string.Format("ProjectManagement/Get"),CurrenUser.JwtToken);
                resp.Data = JsonConvert.DeserializeObject<List<ProjectManagementDTO>>(resp.Data.ToString());
            }
            catch (Exception ex)
            {
                resp.IsSuccess = false;
                resp.Data = "Error";
            }
            return Json(resp);
        }
    }
}
