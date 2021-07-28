using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using RestApiBackend.Contracts.V1.Requests;
using RestApiBackend.Contracts.V1.Responses;
using RestApiBackend.Services;

namespace RestApiBackend.Controllers.V1
{
    public class AuthController : ControllerBaseV1
    {
        private readonly IIdentityService _identityService;

        public AuthController(IIdentityService identityService)
        {
            _identityService = identityService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            var result = await _identityService.RegisterUserAsync(request);

            if (!result.Succeeded)
            {
                return BadRequest(new ApiErrorResponse {Errors = result.Errors});
            }

            var response = new AuthResponse {Token = result.Token, RefreshToken = result.RefreshToken};
            var createdUser = await _identityService.GetUserByUsernameAsync(request.Username);

            return CreatedAtRoute("GetUserById", new {Id = createdUser.Id}, response);
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            var result = await _identityService.LoginAsync(request);

            if (!result.Succeeded)
            {
                return Unauthorized();
            }

            return Ok(new AuthResponse {Token = result.Token, RefreshToken = result.RefreshToken});
        }

        [AllowAnonymous]
        [HttpPost("refreshtoken")]
        public async Task<IActionResult> RefreshToken(RefreshTokenRequest request)
        {
            var result = await _identityService.RefreshToken(request);

            if (!result.Succeeded)
            {
                return BadRequest(new ApiErrorResponse {Errors = result.Errors});
            }

            return Ok(new AuthResponse {Token = result.Token, RefreshToken = result.Token});
        }
    }
}