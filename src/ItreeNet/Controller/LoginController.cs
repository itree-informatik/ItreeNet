using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Mvc;

namespace ItreeNet.Controller
{
    [Route("[controller]")]
    [ApiController]
    public class LoginController : Microsoft.AspNetCore.Mvc.Controller
    {
        [HttpGet("microsoft")]
        public ActionResult Login(string redirectUrl)
        {
            if (string.IsNullOrEmpty(redirectUrl) || !Url.IsLocalUrl(redirectUrl))
                redirectUrl = "/intern";

            var props = new AuthenticationProperties
            {
                RedirectUri = redirectUrl
            };

            return Challenge(props, OpenIdConnectDefaults.AuthenticationScheme);
        }

        [HttpGet("logout")]
        public async Task Logout()
        {
            await HttpContext.SignOutAsync("Cookies");
            var prop = new AuthenticationProperties()
            {
                RedirectUri = "/"
            };
            await HttpContext.SignOutAsync("OpenIdConnect", prop);

        }
    }
}
