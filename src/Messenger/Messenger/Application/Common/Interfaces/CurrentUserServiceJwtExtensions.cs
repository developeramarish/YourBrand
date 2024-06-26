using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;

using IdentityModel;
using IdentityModel.Client;

using Microsoft.IdentityModel.Tokens;

using YourBrand.Identity;

namespace YourBrand.Messenger.Application.Common.Interfaces;

public static class UserContextJwtExtensions
{
    private static DiscoveryDocumentResponse? discoveryDocumentResponse;

    public static async Task SetCurrentUserFromAccessTokenAsync(this ISettableUserContext userContext, string accessToken)
    {
        var httpClient = new HttpClient();

        if (discoveryDocumentResponse is null)
        {
            discoveryDocumentResponse = await httpClient.GetDiscoveryDocumentAsync("https://localhost:5040");
        }

        var issuerSigningKeys = new List<SecurityKey>();

        foreach (var webKey in discoveryDocumentResponse.KeySet.Keys)
        {
            var e = Base64Url.Decode(webKey.E);
            var n = Base64Url.Decode(webKey.N);

            var key = new RsaSecurityKey(new RSAParameters
            { Exponent = e, Modulus = n })
            {
                KeyId = webKey.Kid
            };

            issuerSigningKeys.Add(key);
        }

        var tokenValidationParameters = new TokenValidationParameters()
        {
            ValidAudience = "myapi",
            ValidIssuer = "https://localhost:5040",
            IssuerSigningKeys = issuerSigningKeys
        };

        var claimsPrincipal = new JwtSecurityTokenHandler().ValidateToken(accessToken,
                            tokenValidationParameters, out var rawValidatedToken);

        //INFO: Fix ?
        userContext.SetCurrentUser(claimsPrincipal);
    }
}