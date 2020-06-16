using System;

using Microsoft.Owin;

namespace OAuthServer.Extensions
{
    public static class IOwinRequestExtension
    {
        public static bool TryGetAuthCodeAndCodeVerifier(this IOwinRequest request, out string code_verifier, out string authorization_code)
        {
            try
            {
                code_verifier = request.Query.Get(nameof(code_verifier));
                authorization_code = request.Query.Get(nameof(authorization_code));
                return true;
            }
            catch (Exception)
            {
                code_verifier = authorization_code = default;
                return false;
            }
        }

        public static bool TryGetCodeChallenge(this IOwinRequest request, out string code_challenge, out string code_challenge_method)
        {
            try
            {
                code_challenge = request.Query.Get(nameof(code_challenge));
                code_challenge_method = request.Query.Get(nameof(code_challenge_method));
                return true;
            }
            catch (Exception)
            {
                code_challenge = code_challenge_method = default;
                return false;
            }
        }
    }
}