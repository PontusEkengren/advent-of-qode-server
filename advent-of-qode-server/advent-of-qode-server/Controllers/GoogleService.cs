using Google.Apis.Auth;
using Microsoft.Extensions.Primitives;
using System.Threading.Tasks;

namespace advent_of_qode_server.Controllers
{
    public interface IGoogleService
    {
        public Task<string> GetEmailByGmailToken(StringValues token, string googleAuthIdToken);
    }

    public class GoogleService : IGoogleService
    {
        public  async Task<string> GetEmailByGmailToken(StringValues token, string googleAuthIdToken)
        {
            var result = await GoogleJsonWebSignature.ValidateAsync(token,
            new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new[] { googleAuthIdToken }
            });
            return result.Email;
        }
    }
}