using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using RestApiBackend.Contracts.V1.Requests;
using RestApiBackend.Contracts.V1.Responses;
using RestApiBackend.Controllers.V1;
using RestApiBackend.Services;

namespace RestApiBackend.Contracts.V1
{
    public class AccountController : ControllerBaseV1
    {
        private readonly IIdentityService _identityService;
        private readonly IMapper _mapper;

        public AccountController(IIdentityService identityService, IMapper mapper)
        {
            _identityService = identityService;
            _mapper = mapper;
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAccountInfo(int id, UpdateAccountInfoRequest request)
        {
            var currentUserId = _identityService.GetCurrentUserId();

            if (currentUserId != id || request.Id != currentUserId)
            {
                return Unauthorized();
            }

            var result = await _identityService.UpdateAccountInfoAsync(request);

            if (!result.Succeeded)
            {
                return BadRequest(new ApiErrorResponse(result.Errors));
            }

            return NoContent();
        }

        [HttpPost("{id}/changepassword")]
        public async Task<IActionResult> ChangePassword(int id, ChangePasswordRequest request)
        {
            var currentUserId = _identityService.GetCurrentUserId();

            if (currentUserId != id)
            {
                return Unauthorized();
            }

            var result = await _identityService.ChangePasswordAsync(id, request);

            if (!result.Succeeded)
            {
                return BadRequest(new ApiErrorResponse(result.Errors));
            }

            return Ok(new AuthResponse {Token = result.Token, RefreshToken = result.Token});
        }

        [HttpPost("{id}/resetpassword")]
        public async Task<IActionResult> ResetPassword(int id, ResetPasswordRequest request)
        {
            var currentUserId = _identityService.GetCurrentUserId();

            if (currentUserId != id)
            {
                return Unauthorized();
            }

            var result = await _identityService.ResetPasswordAsync(id, request);

            if (!result.Succeeded)
            {
                return BadRequest(new ApiErrorResponse(result.Errors));
            }

            return Ok(new AuthResponse {Token = result.Token, RefreshToken = result.Token});
        }

        [HttpGet("{id}/resetpasswordtoken")]
        public async Task<IActionResult> GenerateResetPasswordToken(int id)
        {
            var currentUserId = _identityService.GetCurrentUserId();

            if (currentUserId != id)
            {
                return Unauthorized();
            }

            var token = await _identityService.GeneratePasswordResetTokenAsync(id);

            // TODO: Enviar token al email del usuario

            return Ok(new {ResetToken = token});
        }
    }
}