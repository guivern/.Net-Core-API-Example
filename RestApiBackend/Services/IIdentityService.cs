using System.Collections.Generic;
using System.Threading.Tasks;
using RestApiBackend.Contracts.V1.Requests;
using RestApiBackend.Entities;
using RestApiBackend.Helpers;
using RestApiBackend.Services.Models;

namespace RestApiBackend.Services
{
    public interface IIdentityService
    {
        long GetCurrentUserId();
        Task<AuthServiceResult> RegisterUserAsync(RegisterRequest dto);
        Task<ServiceResult> UpdateUserAsync(UpdateUserRequest dto);
        Task<ServiceResult> UpdateAccountInfoAsync(UpdateAccountInfoRequest dto);
        Task<PagedList<User>> GetUsersAsync(int pageSize, int pageNumber, string filter, List<string> orderBy, bool includeRoles = false);
        Task<User> GetUserByIdAsync(long id, bool includeRoles = false, bool tracking = false);
        Task<User> GetUserByUsernameAsync(string useraname);
        Task<List<Role>> GetRolesAsync();
        Task<AuthServiceResult> LoginAsync(LoginRequest dto);
        Task<AuthServiceResult> ChangePasswordAsync(long userId, ChangePasswordRequest dto);
        Task<AuthServiceResult> ResetPasswordAsync(long userId, ResetPasswordRequest dto);
        Task<string> GeneratePasswordResetTokenAsync(long userId);
        Task<bool> DeleteUserAsync(long userId);
        Task<bool> UserExistsAsync(long userId);
        Task<AuthServiceResult> RefreshToken(RefreshTokenRequest dto);
    }
}