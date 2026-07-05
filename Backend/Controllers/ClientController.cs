using Backend.DTOs;
using Backend.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ClientController:ControllerBase
    {
        private readonly IClientService _clientService;
        public ClientController(IClientService clientService)
        {
            _clientService = clientService;
        }
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var clients = await _clientService.GetAllAsync();
            return Ok(clients);
        }

        [HttpGet("count")]
        public async Task<IActionResult> GetCount()
        {
            var count = await _clientService.GetCountAsync();
            return Ok(new { count });
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string q)
        {
            var clients = await _clientService.SearchAsync(q);
            return Ok(clients);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var client = await _clientService.GetByIdAsync(id);
            if (client == null) return NotFound();
            return Ok(client);
        }

        [HttpGet("email/{email}")]
        public async Task<IActionResult> GetByEmail(string email)
        {
            var client = await _clientService.GetByEmailAsync(email);
            if (client == null) return NotFound();
            return Ok(client);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Commercial")]
        public async Task<IActionResult> Create([FromBody] CreateClientRequest request)
        {
            try
            {
                var client = await _clientService.CreateAsync(request);
                return CreatedAtAction(nameof(GetById), new { id = client.Id }, client);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Commercial")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateClientRequest request)
        {
            var client = await _clientService.UpdateAsync(id, request);
            if (client == null) return NotFound();
            return Ok(client);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _clientService.DeleteAsync(id);
            if (!result) return NotFound();
            return NoContent();
        }
    }
}
