using ChatApi.Models;
using JWT;
using JWT.Serializers;
using JWT.Algorithms;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using System;
using System.Text.Json;

namespace ChatApi.Helpers
{
    public class AuthHelper : IAuthHelper
    {
        public void AuthenticateRequest(HttpRequest request, string jwtSecret, out string payload)
        {
            payload = null;
            string authorization;

            try { authorization = request.Headers["Authorization"][0]; }
            catch { authorization = String.Empty; }

            if (String.IsNullOrWhiteSpace(authorization)) throw new AuthenticationException("Missing authorization header");

            string token;

            try { token = authorization.Split(new string[] { " " }, StringSplitOptions.None)[1]; }
            catch { token = String.Empty; }

            if (String.IsNullOrWhiteSpace(token)) throw new AuthenticationException("Invalid authorization header");

            try
            {
                IJsonSerializer serializer = new JsonNetSerializer();

                payload = new JwtDecoder(
                    serializer,
                    new JwtValidator(serializer, new UtcDateTimeProvider()),
                    new JwtBase64UrlEncoder(),
                    new HMACSHA256Algorithm()
                ).Decode(token, jwtSecret, true);
            }
            catch (InvalidTokenPartsException)
            {
                throw new AuthenticationException("Invalid authorization token");
            }
            catch (TokenExpiredException)
            {
                throw new AuthenticationException("Authorization token has expired");
            }
            catch (SignatureVerificationException)
            {
                throw new AuthenticationException("Invalid authorization token signature");
            }
        }

        public Identity GetRequestIdentity(string authPayload)
        {
            JToken t = JObject.Parse(authPayload)["identity"];
            if (t == null) throw new IdentityException();

            Identity identity = JsonSerializer.Deserialize<Identity>(t.ToString());
            if (identity.UserName == null) throw new IdentityException();

            return identity;
        }
    }
}
