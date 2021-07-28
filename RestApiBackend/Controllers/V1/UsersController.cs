using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using RestApiBackend.Contracts.V1.Requests;
using RestApiBackend.Contracts.V1.Responses;
using RestApiBackend.Helpers;
using RestApiBackend.Services;

namespace RestApiBackend.Controllers.V1
{
    public class UsersController : ControllerBaseV1
    {
        private readonly IIdentityService _identityService;
        private readonly IMapper _mapper;

        public UsersController(IIdentityService identityService, IMapper mapper)
        {
            _identityService = identityService;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] string filter,
            [FromQuery] List<string> orderBy,
            [FromQuery] int pageSize = Constants.DEFAULT_PAGE_SIZE,
            [FromQuery] int pageNumber = Constants.DEFAULT_PAGE_NUMBER)
        {
            var users = await _identityService.GetUsersAsync(pageSize, pageNumber, filter, orderBy, true);
            var dto = _mapper.Map<List<UserDtoResponse>>(users);

            Response.AddPaginationHeaders(pageNumber, pageSize, users.TotalPages, users.TotalCount);

            return Ok(dto);
        }

        [HttpGet("{id}", Name = "GetUserById")]
        public async Task<IActionResult> GetById(int id)
        {
            var user = await _identityService.GetUserByIdAsync(id, true);

            if (user == null)
            {
                return NotFound();
            }

            var dto = _mapper.Map<UserDtoResponse>(user);

            return Ok(dto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UpdateUserRequest request)
        {
            var result = await _identityService.UpdateUserAsync(request);

            if (!result.Succeeded)
            {
                return BadRequest(new ApiErrorResponse(result.Errors));
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var userExists = await _identityService.UserExistsAsync(id);

            if (!userExists)
            {
                return NotFound();
            }

            await _identityService.DeleteUserAsync(id);

            return NoContent();
        }
    }
}