using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Spoti.Pages
{
    [AllowAnonymous]
    public class LoginModel : PageModel
    {
        public IActionResult OnGetLogin()
        {
            var authenticationProperties = new AuthenticationProperties { RedirectUri = "/" };
            return Challenge(authenticationProperties, "Spotify");
        }
    }
}
