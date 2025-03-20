using EMS.UI.Models;
using EMS.UI.Services;
using EMS.UI.Utilities;

namespace EMS.UI.Controllers
{
    public class AuthController : BaseController
    {
        private readonly IConfiguration _configuration;
        public AuthController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string Username, string Pass, string RecaptchaToken)
        {

            // Validate reCAPTCHA
            if (RecaptchaToken == null)
            {
                ViewBag.errormsg = "Recaptacha is missing";
                return View();
            }
            string secretkey = _configuration["ReCaptchaSettings:SecretKey"];
            bool success = await ReCaptchaService.VerifyCaptchaV2(RecaptchaToken, secretkey);
            if (!success)
            {
                ViewBag.errormsg = "unvalid recaptcha";
                return View();
            }

            var loginModel = new LoginModel
            {
                Username = Username,
                Pass = Pass
            };


            var apiResponse = await ApiUtility.GetApi($"Auth/Login?username={Uri.EscapeDataString(loginModel.Username)}&password={Uri.EscapeDataString(loginModel.Pass)}");


            if (apiResponse.IsSuccess)
            {
                var data = Convert.ToString(apiResponse.Data);
                HttpContext.Session.SetString("User", data);

                TempData["LoginSuccess"] = "Login Successful!";
                return RedirectToAction("Index", "Home");

            }
            else
            {
                ViewBag.errormsg = apiResponse.Message;
                return View();
            }
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        
        public IActionResult SignOut()
        {
            HttpContext.Session.Clear();
            
            return RedirectToAction("Login","Auth");
        }
    }
}
