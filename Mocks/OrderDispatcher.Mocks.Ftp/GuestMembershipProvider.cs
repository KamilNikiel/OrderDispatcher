using FubarDev.FtpServer.AccountManagement;
using System.Security.Claims;

namespace OrderDispatcher.Mocks.Ftp
{
    public class GuestMembershipProvider : IMembershipProvider
    {
        private const string Guest = "guest";
        private const string AuthenticationType = "custom";

        public Task<MemberValidationResult> ValidateUserAsync(string username, string password)
        {
            if (username == Guest && password == Guest)
            {
                var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, username) }, AuthenticationType);
                return Task.FromResult(new MemberValidationResult(
                    MemberValidationStatus.AuthenticatedUser,
                    new ClaimsPrincipal(identity)));
            }

            return Task.FromResult(new MemberValidationResult(MemberValidationStatus.InvalidLogin));
        }
    }
}