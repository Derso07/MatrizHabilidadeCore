using MatrizHabilidadeCore.ViewModel;
using MatrizHabilidadeDataBaseCore.Services;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace MatrizHabilidadeCore.Services
{
    public class CookieService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CookieService(IHttpContextAccessor httpContextAcessor)
        {
            _httpContextAccessor = httpContextAcessor;
        }

        public void SetCookie(CookieViewModel cookie)
        {
            string value = JsonConvert.SerializeObject(cookie);
            string encryptedValue = Encrypting.Encrypt(value);
            _httpContextAccessor.HttpContext.Response.Cookies.Append(cookie.Usu_nome, encryptedValue);
        }

        public string GetCookie()
        {
            var cookie = _httpContextAccessor.HttpContext.Request.Cookies;
            CookieViewModel model = JsonConvert.DeserializeObject<CookieViewModel>(Encrypting.Decrypt(cookie.ToString()));
            return model.ToString();
        }
    }
}
