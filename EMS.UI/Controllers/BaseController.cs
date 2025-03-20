
global using Microsoft.AspNetCore.Mvc;
global using Newtonsoft.Json;
using EMS.Core.Models;

namespace EMS.UI.Controllers
{
    public class BaseController : Controller
    {
		private UserDTO model;
		public UserDTO CurrenUser
		{
			get
			{
				string session = HttpContext.Session.GetString("User");
				if (session != null)
				{
					model = JsonConvert.DeserializeObject<UserDTO>(session);
				}
				return model;
			}
			set
			{
				model = value;
			}
		}
		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
		}
	}
}
