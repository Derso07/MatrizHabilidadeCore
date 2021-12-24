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

        public void SetCookie<T>(string name, T value)
        {
            string cookieValue = JsonConvert.SerializeObject(value);
            string encryptedValue = Encrypting.Encrypt(cookieValue);
            _httpContextAccessor.HttpContext.Response.Cookies.Append(name, encryptedValue);
        }

        public T GetCookie<T>(string name)
        {
            var cookie = _httpContextAccessor.HttpContext.Request.Cookies[name];
            return JsonConvert.DeserializeObject<T>(Encrypting.Decrypt(cookie));
        }
    }
}
