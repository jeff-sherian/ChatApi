using ChatApi.Helpers;
using ChatApi.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace ChatApi.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate Next;
        private readonly IConfiguration Configuration;
        private readonly IAuthHelper AuthHelper;
        private readonly ILogger<ExceptionMiddleware> Logger;
        
        public ExceptionMiddleware(RequestDelegate next, IConfiguration configuration, IAuthHelper authHelper, ILogger<ExceptionMiddleware> logger)
        {
            this.Next = next;
            this.Configuration = configuration;
            this.AuthHelper = authHelper;
            this.Logger = logger;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                string authPayload;

                // The following would eventually get replaced with built-in JWT Authentication in Startup.cs
                this.AuthHelper.AuthenticateRequest(httpContext.Request, this.Configuration["JwtSecret"], out authPayload);
                httpContext.Items["Identity"] = this.AuthHelper.GetRequestIdentity(authPayload);

                await this.Next(httpContext);
            }
            catch (AuthenticationException na)
            {
                await HandleExceptionAsync(httpContext, HttpStatusCode.Unauthorized, "NotAuthorizedException", na.Message);
            }
            catch (IdentityException)
            {
                await HandleExceptionAsync(httpContext, HttpStatusCode.Forbidden, "IdentityException", "Unrecognized identity");
            }
            catch (BadRequestException br)
            {
                await HandleExceptionAsync(httpContext, HttpStatusCode.BadRequest, "BadRequestException", br.Message);
            }
            catch (Exception ex)
            {
                this.Logger.LogError(ex, "An unexpected error has occured");
                await HandleExceptionAsync(httpContext, HttpStatusCode.InternalServerError, "Exception", ReasonPhrases.GetReasonPhrase((int)HttpStatusCode.InternalServerError));
            }
        }

        private Task HandleExceptionAsync(HttpContext context, HttpStatusCode statusCode, string source, string message)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;

            return context.Response.WriteAsync(
                JsonSerializer.Serialize(
                    new ExceptionResponse()
                    {
                        StatusCode = context.Response.StatusCode,
                        Source = source,
                        Message = message
                    }
                )
            );
        }
    }
}
