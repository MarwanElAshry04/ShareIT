using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;

namespace ShareIT.Controllers
{
    /// <summary>
    /// Handles the navbar language toggle. Writes the culture cookie that
    /// CookieRequestCultureProvider reads on every subsequent request.
    /// </summary>
    public class CultureController : Controller
    {
        private static readonly string[] Supported = { "en", "ar" };

        [HttpGet]
        public IActionResult Set(string culture, string returnUrl)
        {
            if (!string.IsNullOrEmpty(culture) && Supported.Contains(culture))
            {
                Response.Cookies.Append(
                    CookieRequestCultureProvider.DefaultCookieName,
                    CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                    new CookieOptions
                    {
                        Expires = DateTimeOffset.UtcNow.AddYears(1),
                        IsEssential = true
                    });
            }

            // LocalRedirect rejects absolute URLs, so returnUrl can't be used as an open redirect.
            return LocalRedirect(string.IsNullOrEmpty(returnUrl) ? "~/" : returnUrl);
        }
    }
}
