using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace ChatApi.Middleware
{
    public class WebSocketsMiddleware
    {
        private readonly RequestDelegate _next;

        public WebSocketsMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            /*
            Check whether a token was included in the Authorization header, or as a query string parameter
            If it is in the header no action needs to be taken
            If it is in the query string we need to manually add it to the header for downstream processing
                This will be necessary when transitioning to the built-in .NET Core JWT Authorization
            */

            string authorization;

            try 
            { 
                authorization = httpContext.Request.Headers["Authorization"][0]; 
            }
            catch
            {
                string accessToken;

                try 
                { 
                    accessToken = httpContext.Request.Query["access_token"][0];
                    authorization = String.Format("Bearer {0}", accessToken);
                }
                catch 
                { 
                    authorization = String.Empty;
                }

                httpContext.Request.Headers.Add("Authorization", authorization);
            }      

            await _next(httpContext);
        }
    }
}
