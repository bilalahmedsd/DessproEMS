using EMS.Core.Models;
using EMS.UI.CustomFilter;
using EMS.UI.Utilities;
using Microsoft.AspNetCore.Mvc;
using NuGet.Protocol;

namespace EMS.UI.Controllers
{
    [AuthorizationFilter("1")]
    public class UnitController : BaseController
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Add(UnitDTO unit)
        {
            ResponseModel resp = new ResponseModel();
            try
            {
                resp = await ApiUtility.PostApi(string.Format("Unit/Insert"), unit.ToJson(), CurrenUser.JwtToken);
                resp.Data = JsonConvert.DeserializeObject<UnitDTO>(resp.Data.ToString());
            }
            catch (Exception ex)
            {
                resp.IsSuccess = false;
                resp.Data = "Error";
            }
            return Json(resp);
        }

        [HttpPost]
        public IActionResult Edit(UnitDTO unit)
        {
            return View();
        }

        [HttpGet]
        public IActionResult Delete(int id)
        {
            return View();
        }
    }
}
