using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using RestApiBackend.Contracts.V1.Responses;
using RestApiBackend.Services;

namespace RestApiBackend.Controllers.V1
{
    public class RolesController: ControllerBaseV1
    {
        private readonly IIdentityService _identityService;
        private readonly IMapper _mapper;

        public RolesController(IIdentityService identityService, IMapper mapper)
        {
            _identityService = identityService;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var roles = await _identityService.GetRolesAsync();
            var dto = _mapper.Map<List<RoleDtoResponse>>(roles);

            return Ok(dto);
        }
    }
}