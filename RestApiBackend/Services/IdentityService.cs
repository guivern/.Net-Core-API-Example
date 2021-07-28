using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using RestApiBackend.Contracts.V1.Requests;
using RestApiBackend.Data;
using RestApiBackend.Entities;
using RestApiBackend.Helpers;
using RestApiBackend.Services.Models;

namespace RestApiBackend.Services
{
    public class IdentityService : IIdentityService
    {
        private readonly DataContext _context;
        private readonly UserManager<User> _userManager;
        private readonly IConfiguration _configuration;
        private readonly SignInManager<User> _signIngManager;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public IdentityService(UserManager<User> userManager, IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor, DataContext context, SignInManager<User> signIngManager)
        {
            _context = context;
            _userManager = userManager;
            _configuration = configuration;
            _signIngManager = signIngManager;
            _httpContextAccessor = httpContextAccessor;
        }

        public long GetCurrentUserId()
        {
            if (_httpContextAccessor.HttpContext != null)
                return int.Parse(_httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);

            return 0;
        }

        public async Task<AuthServiceResult> RegisterUserAsync(RegisterRequest dto)
        {
            var userRoles = new List<UserRole>();
            var userAvailability = await ValidateUserAvailabilityAsync(dto.Username, dto.Email);
            var rolesValidation = await ValidateRolesAsync(dto.RolesIds);
            var newUser = new User {Email = dto.Email, UserName = dto.Username};

            if (!userAvailability.Succeeded)
            {
                return new AuthServiceResult {Succeeded = false, Errors = userAvailability.Errors};
            }
            
            if (!rolesValidation.Succeeded)
            {
                return new AuthServiceResult {Succeeded = false, Errors = rolesValidation.Errors};
            }

            var result = await _userManager.CreateAsync(newUser, dto.Password);

            if (!result.Succeeded)
            {
                return new AuthServiceResult
                {
                    Succeeded = false, 
                    Errors = result.Errors.Select(x => x.Description).ToList()
                };
            }

            foreach (var roleId in dto.RolesIds)
            {
                userRoles.Add(new UserRole {RoleId = roleId, UserId = newUser.Id});
            }

            await _context.UserRoles.AddRangeAsync(userRoles);
            await _context.SaveChangesAsync();

            var jwtToken = await GenerateJwtToken(newUser);

            return new AuthServiceResult
            {
                Succeeded = true, 
                Token = jwtToken.Token, 
                RefreshToken = jwtToken.RefreshToken
            };
        }

        public async Task<ServiceResult> UpdateUserAsync(UpdateUserRequest dto)
        {
            var userAvailability = await ValidateUserAvailabilityAsync(dto.Username, dto.Email, dto.Id);
            var rolesValidation = await ValidateRolesAsync(dto.RolesIds);
            var userRoles = new List<UserRole>();

            if (!userAvailability.Succeeded)
            {
                return new ServiceResult {Succeeded = false, Errors = userAvailability.Errors};
            }

            if (!rolesValidation.Succeeded)
            {
                return new ServiceResult {Succeeded = false, Errors = rolesValidation.Errors};
            }

            foreach (var roleId in dto.RolesIds)
            {
                userRoles.Add(new UserRole {RoleId = roleId, UserId = dto.Id});
            }

            var user = await GetUserByIdAsync(dto.Id, true, true);
            
            user.UserName = dto.Username;
            user.Email = dto.Email;
            user.UserRoles = userRoles;
            user.LastModified = DateTime.Now;

            var result = await _context.SaveChangesAsync();

            return new ServiceResult {Succeeded = result > 0};
        }

        public async Task<ServiceResult> UpdateAccountInfoAsync(UpdateAccountInfoRequest dto)
        {
            var userAvailability = await ValidateUserAvailabilityAsync(dto.Username, dto.Email, dto.Id);

            if (!userAvailability.Succeeded)
            {
                return new ServiceResult {Succeeded = false, Errors = userAvailability.Errors};
            }

            var user = await GetUserByIdAsync(dto.Id);
            
            user.UserName = dto.Username;
            user.Email = dto.Email;
            user.LastModified = DateTime.Now;

            _context.Update(user);
            
            var result = await _context.SaveChangesAsync();

            return new ServiceResult {Succeeded = result > 0};
        }
        
        public async Task<User> GetUserByIdAsync(long id, bool includeRoles = false, bool tracking = false)
        {
            var query = _context.Users.Where(x => !x.IsDeleted);

            if (!tracking)
                query = query.AsNoTracking();
            
            if (includeRoles)
                query = query.Include(x => x.UserRoles).ThenInclude(x => x.Role);

            return await query.SingleOrDefaultAsync(x => x.Id == id);
        }

        public async Task<PagedList<User>> GetUsersAsync(int pageSize, int pageNumber, string filter, List<string> orderBy, bool includeRoles = false)
        {
            var query = _context.Users.AsNoTracking().AsQueryable();

            if (includeRoles)
            {
                query = query.Include(x => x.UserRoles).ThenInclude(x => x.Role);
            }

            query = query.Where(x => !x.IsDeleted);
            query = query.Filter<User>(filter, new List<string> {nameof(User.Email), nameof(User.UserName)});
            query = query.Sort<User>(orderBy);

            return await PagedList<User>.CreateAsync(query, pageNumber, pageSize);
        }

        public async Task<User> GetUserByUsernameAsync(string useraname)
        {
            return await _context.Users.AsNoTracking()
                .Where(x => !x.IsDeleted)
                .Where(x => x.NormalizedUserName == useraname.Trim().ToUpper() || x.NormalizedEmail == useraname.Trim().ToUpper())
                .SingleOrDefaultAsync();
        }

        public async Task<AuthServiceResult> LoginAsync(LoginRequest dto)
        {
            var user = await GetUserByUsernameAsync(dto.Username);

            if (user == null)
            {
                return new AuthServiceResult
                {
                    Succeeded = false,
                    Errors = new List<string> {$"User {dto.Username} does not exists"}
                };
            }

            var result = await _signIngManager.CheckPasswordSignInAsync(user, dto.Password, false);

            if (!result.Succeeded)
            {
                return new AuthServiceResult {Succeeded = false};
            }
            
            var jwtToken = await GenerateJwtToken(user);
            
            return new AuthServiceResult
            {
                Succeeded = jwtToken.Succeeded,
                Token = jwtToken.Token,
                RefreshToken = jwtToken.RefreshToken
            };
        }

        public async Task<List<Role>> GetRolesAsync()
        {
            return await _context.Roles.ToListAsync();
        }

        public async Task<AuthServiceResult> ChangePasswordAsync(long userId, ChangePasswordRequest dto)
        {
            var user = await GetUserByIdAsync(userId);
            var currentPasswordValidation = await _signIngManager.CheckPasswordSignInAsync(user, dto.CurrentPassword, false);

            if (!currentPasswordValidation.Succeeded)
            {
                return new AuthServiceResult
                {
                    Succeeded = false,
                    Errors = new List<string> {$"Current password is not valid"}
                };
            }

            if (dto.CurrentPassword.Equals(dto.NewPassword))
            {
                return new AuthServiceResult
                {
                    Succeeded = false,
                    Errors = new List<string> {"The new password must be different to current password"}
                };
            }

            var result = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);

            if (!result.Succeeded)
            {
                return new AuthServiceResult
                {
                    Succeeded = false,
                    Errors = result.Errors.Select(x => x.Description).ToList()
                };
            }

            var jwtToken = await GenerateJwtToken(user);
            
            return new AuthServiceResult()
            {
                Succeeded = jwtToken.Succeeded,
                Token = jwtToken.Token,
                RefreshToken = jwtToken.RefreshToken
            };
        }

        public async Task<AuthServiceResult> ResetPasswordAsync(long userId, ResetPasswordRequest dto)
        {
            var user = await GetUserByIdAsync(userId);
            var result = await _userManager.ResetPasswordAsync(user, dto.Token, dto.NewPassword);

            if (!result.Succeeded)
            {
                return new AuthServiceResult
                {
                    Succeeded = false,
                    Errors = result.Errors.Select(x => x.Description).ToList()
                };
            }

            var jwtToken = await GenerateJwtToken(user);

            return new AuthServiceResult
            {
                Succeeded = jwtToken.Succeeded,
                Token = jwtToken.Token,
                RefreshToken = jwtToken.RefreshToken
            };
        }

        public async Task<string> GeneratePasswordResetTokenAsync(long userId)
        {
            var user = await GetUserByIdAsync(userId);
            return await _userManager.GeneratePasswordResetTokenAsync(user);
        }

        public async Task<bool> DeleteUserAsync(long userId)
        {
            var user = await GetUserByIdAsync(userId);
            user.IsDeleted = true;
            user.LastModified = DateTime.Now;

            _context.Update(user);

            var result = await _context.SaveChangesAsync();

            return result  > 0;
        }

        public async Task<bool> UserExistsAsync(long userId)
        {
            return await _context.Users.AnyAsync(x => !x.IsDeleted && x.Id == userId);
        }
        
        public async Task<AuthServiceResult> RefreshToken(RefreshTokenRequest dto)
        {
            // Source: https://dev.to/moe23/refresh-jwt-with-refresh-tokens-in-asp-net-core-5-rest-api-step-by-step-3en5?signin=true
            var jwtTokenHandler = new JwtSecurityTokenHandler();
            try
            {
                var principal = jwtTokenHandler.ReadJwtToken(dto.Token);
                var storedRefreshToken = await _context.RefreshTokens.FirstOrDefaultAsync(x => x.Token == dto.RefreshToken);
                var jti = principal.Claims.SingleOrDefault(x => x.Type == JwtRegisteredClaimNames.Jti).Value;
                
                if(storedRefreshToken == null)
                {
                    return new AuthServiceResult
                    {
                        Errors = new List<string>() {"Refresh token doesnt exist"},
                        Succeeded = false
                    };
                }

                if(DateTime.UtcNow > storedRefreshToken.ExpiryDate)
                {
                    return new AuthServiceResult
                    {
                        Errors = new List<string>() {"Refresh token has expired, user needs to relogin"},
                        Succeeded = false
                    };
                }

                if(storedRefreshToken.IsUsed || storedRefreshToken.IsRevoked)
                {
                    return new AuthServiceResult
                    {
                        Errors = new List<string>() {"Token has been used or revoked"},
                        Succeeded = false
                    };
                }

                if(storedRefreshToken.JwtId != jti)
                {
                   return new AuthServiceResult
                   {
                        Errors = new List<string>() {"The token does not mateched the saved token"},
                        Succeeded = false
                    };
                }

                storedRefreshToken.IsUsed = true;
                
                _context.RefreshTokens.Update(storedRefreshToken);
                await _context.SaveChangesAsync();

                var user = await GetUserByIdAsync(storedRefreshToken.UserId);
                
                return await GenerateJwtToken(user);
            }
            catch(Exception)
            {
                return new AuthServiceResult
                {
                    Succeeded = false,
                    Errors = new List<string> {"Token is not valid"}
                };
            }
        }
        
        private async Task<AuthServiceResult> GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Auth0:key"]);
            var tokenExpirationInHours = double.Parse(_configuration["Auth0:TokenExpirationInHours"]);
            var refreshTokenExpirationInHours = double.Parse(_configuration["Auth0:RefreshTokenExpirationInHours"]);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Email, user.Email),
            };

            var rolesIds = _context.UserRoles.Where(ur => ur.UserId == user.Id).Select(ur => ur.Role.Id).ToList();
            
            foreach (var roleId in rolesIds)
            {
                claims.Add(new Claim(ClaimTypes.Role, roleId.ToString()));
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(tokenExpirationInHours),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256),
                Audience = _configuration["Auth0:Audience"],
                Issuer = _configuration["Auth0:Domain"]
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var jwtToken = tokenHandler.WriteToken(token);
            
            var refreshToken = new RefreshToken(){
                JwtId = token.Id,
                IsUsed = false,
                UserId = user.Id,
                AddedDate = DateTime.UtcNow,
                ExpiryDate = DateTime.UtcNow.AddHours(refreshTokenExpirationInHours),
                IsRevoked = false,
                Token = RandomString(25) + Guid.NewGuid()
            };

            await _context.RefreshTokens.AddAsync(refreshToken);
            await _context.SaveChangesAsync();

            return new AuthServiceResult {
                Token = jwtToken,
                Succeeded = true,
                RefreshToken = refreshToken.Token
            };
        }
        
        private string RandomString(int length)
        {
            var random = new Random();
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
        
        private async Task<ServiceResult> ValidateRolesAsync(List<long> rolesIds)
        {
            var result = new ServiceResult {Succeeded = true};

            foreach (var roleId in rolesIds)
            {
                var exists = await _context.Roles.AnyAsync(r => r.Id == roleId);

                if (exists) continue;
                result.Succeeded = false;
                result.Errors.Add($"Role with Id {roleId} does not exist");
            }

            return result;
        }

        private async Task<ServiceResult> ValidateUserAvailabilityAsync(string username, string email, long? userIdEditing = null)
        {
            var result = new ServiceResult {Succeeded = true};

            var emailIsAvailable = !await _context.Users
                .Where(x => !x.IsDeleted)
                .AnyAsync(x => x.NormalizedEmail == email.Trim().ToUpper() && x.Id != userIdEditing);

            var usernameIsAvailable = !await _context.Users
                .Where(x => !x.IsDeleted)
                .AnyAsync(x => x.NormalizedUserName == username.Trim().ToUpper() && x.Id != userIdEditing);

            if (!emailIsAvailable)
            {
                result.Succeeded = false;
                result.Errors.Add("User with this email already exists");
            }

            if (!usernameIsAvailable)
            {
                result.Succeeded = false;
                result.Errors.Add("User with this username already exists");
            }

            return result;
        }
    }
}