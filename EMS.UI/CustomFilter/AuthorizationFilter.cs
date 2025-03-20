using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using EMS.Core.Models;

namespace EMS.UI.CustomFilter
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
	public class AuthorizationFilter : Attribute, IAuthorizationFilter
	{
		private string[] Role { get; set; }
		public AuthorizationFilter(string Role)
		{
			this.Role = Role.Split(',');
		}
		public void OnAuthorization(AuthorizationFilterContext context)
		{
			string userstring = context.HttpContext.Session.GetString("Users");
			UserDTO? user;
			if (!string.IsNullOrEmpty(userstring))
			{
				user = JsonConvert.DeserializeObject<UserDTO>(userstring);
				if (Role.Contains(Convert.ToString(user?.FkUserRoleId)))
				{
					return;
				}
				else
				{
					context.Result = new RedirectResult("/Error/UnAuthroize");
				}
			}
			else
			{
				if (context.HttpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
				{
					ResponseModel resp = new ResponseModel();
					resp.Message = "session expired";
					resp.IsSuccess = false;
					context.Result = new JsonResult(resp);
				}
				else
				{
					context.Result = new RedirectResult("/User/Login");
				}
			}
		}
	}
}
