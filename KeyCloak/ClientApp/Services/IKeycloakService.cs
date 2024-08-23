using ClientApp.Models;
using Refit;

namespace ClientApp.RefitServices
{
    public interface IKeycloakService
    {
        [Headers("Content-Type: application/x-www-form-urlencoded")]
        [Post("/realms/MyRealm/protocol/openid-connect/token")]
        Task<Token> TokenAl([Body(BodySerializationMethod.UrlEncoded)] AuthApi authApi);
    }
}
