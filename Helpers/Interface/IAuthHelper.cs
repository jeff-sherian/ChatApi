using ChatApi.Models;
using Microsoft.AspNetCore.Http;

namespace ChatApi.Helpers
{
    public interface IAuthHelper
    {
        void AuthenticateRequest(HttpRequest request, string jwtSecret, out string payload);
        Identity GetRequestIdentity(string authPayload);
    }
}
