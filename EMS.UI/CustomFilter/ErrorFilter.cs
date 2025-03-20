using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;

namespace EMS.UI.CustomFilter
{
	public class ErrorFilter : Attribute, IExceptionFilter
	{
		public void OnException(ExceptionContext context)
		{
			context.Result = new RedirectResult("/Error");
		}
	}
}
