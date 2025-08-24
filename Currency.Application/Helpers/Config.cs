using IdentityModel;
using IdentityServer4.Models;
using IdentityServer4.Test;
using System.Security.Claims;

namespace Currency.Application.Helpers
{
    public static class Config
    {
        public static IEnumerable<Client> Clients =>
        [
            new Client
            {
                ClientId = "my_client",
                AllowedGrantTypes = IdentityServer4.Models.GrantTypes.ResourceOwnerPassword,
                ClientSecrets = { new Secret("my_secret".ToSha256()) },
                AllowedScopes = { "currency_api", "openid", "profile" },
            }
         ];

        public static IEnumerable<ApiScope> ApiScopes =>
        [
            new ApiScope("currency_api", "Currency API")
            {
                UserClaims =
                {
                    "currency_provider",
                    JwtClaimTypes.Role
                }
            }
        ];

        public static IEnumerable<ApiResource> ApiResources =>
        [
            new ApiResource("currency_api", "Currency API")
            {
                Scopes = { "currency_api" },
                UserClaims =
                {
                    "currency_provider",
                    JwtClaimTypes.Role,
                }
            }
        ];

        public static IEnumerable<IdentityResource> IdentityResources =>
        [
            new IdentityResources.OpenId(),
            new IdentityResources.Profile(),
            new IdentityResource("roles", "User roles", new[] { JwtClaimTypes.Role })

         ];

        public static List<TestUser> Users =>
        [
            new TestUser
            {
                SubjectId = "1",
                Username = "user1",
                Password = "password1",
                Claims =
                [
                    new Claim("currency_provider", "openexchange"),
                ]
            },
            new TestUser
            {
                SubjectId = "2",
                Username = "user2",
                Password = "password2",
                Claims =
                [
                    new Claim("currency_provider", "frankfurter"),
                    new Claim(JwtClaimTypes.Role, "Admin")
                ]
            }
        ];
    }
}
