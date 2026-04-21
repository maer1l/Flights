using Flights.Areas.Identity.Data;
using Flights.Data;
using Flights.Models;
using Flights.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Flights.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class FlightsController : ControllerBase
    {
        private readonly FlightsContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly RoleManager<IdentityRole> _roleManager;

        public FlightsController(FlightsContext context, UserManager<ApplicationUser> userManager, IConfiguration configuration, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _configuration = configuration;
            _roleManager = roleManager;
        }

        [Route("Register")]
        [HttpPost]
        public async Task<ActionResult> Register(IdentityViewModel model)
        {
            //строка в которой решается какой юзер регистрируется
            string _role = "Editor";//Admin, User, Editor

            if (_role == "Admin")
                if (!await _roleManager.RoleExistsAsync("Editor"))
                    await _roleManager.CreateAsync(new IdentityRole("Editor"));

            if (!await _roleManager.RoleExistsAsync(_role))
                await _roleManager.CreateAsync(new IdentityRole(_role));

            var user = new ApplicationUser() { Email = model.Username, UserName = model.Username };
            var result = await _userManager.CreateAsync(user, model.Password); // создали пользователя
            if (result.Succeeded) // если все успешно
            {

                await _userManager.AddToRoleAsync(user, _role);
                if(_role == "Admin")
                    await _userManager.AddToRoleAsync(user, "Editor");

                var guid = Guid.NewGuid().ToString();
                // https://datatracker.ietf.org/doc/html/rfc7519#section-4
                var claims = new List<Claim> {
                    new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                    new Claim(JwtRegisteredClaimNames.Jti, guid),
                    new Claim(JwtRegisteredClaimNames.NameId, user.Id)
                };

                var roles = await _userManager.GetRolesAsync(user);

                foreach (var role in roles)
                {
                    var roleClaim = new Claim(ClaimTypes.Role, role);
                    claims.Add(roleClaim);
                }

                var signingKey = new SymmetricSecurityKey(
                  Encoding.UTF8.GetBytes(_configuration["Jwt:SigningKey"]));

                int expiryInMinutes = Convert.ToInt32(_configuration["Jwt:ExpiryInMinutes"]);

                var token = new JwtSecurityToken(
                  issuer: _configuration["Jwt:Site"],
                  audience: _configuration["Jwt:Site"],
                  claims: claims,
                  expires: DateTime.UtcNow.AddMinutes(expiryInMinutes),
                  signingCredentials: new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256)
                );

                return Ok(
                  new
                  {
                      access_token = new JwtSecurityTokenHandler().WriteToken(token),
                      userName = model.Username,
                      expiration = token.ValidTo
                  });
            }

            return Unauthorized();
        }

        [Route("Login")] // /login
        [HttpPost]
        public async Task<ActionResult> Login(IdentityViewModel model)
        {
            var user = await _userManager.FindByNameAsync(model.Username);
            if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
            {
                var guid = Guid.NewGuid().ToString();
                // https://datatracker.ietf.org/doc/html/rfc7519#section-4
                var claims = new List<Claim> {
                    new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                    new Claim(JwtRegisteredClaimNames.Jti, guid),
                    new Claim(JwtRegisteredClaimNames.NameId, user.Id)
                };

                var roles = await _userManager.GetRolesAsync(user);

                foreach (var role in roles)
                {
                    var roleClaim = new Claim(ClaimTypes.Role, role);
                    claims.Add(roleClaim);
                }

                var signingKey = new SymmetricSecurityKey(
                  Encoding.UTF8.GetBytes(_configuration["Jwt:SigningKey"]));

                int expiryInMinutes = Convert.ToInt32(_configuration["Jwt:ExpiryInMinutes"]);

                var token = new JwtSecurityToken(
                  issuer: _configuration["Jwt:Site"],
                  audience: _configuration["Jwt:Site"],
                  claims: claims,
                  expires: DateTime.UtcNow.AddMinutes(expiryInMinutes),
                  signingCredentials: new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256)
                );

                return Ok(
                  new
                  {
                      access_token = new JwtSecurityTokenHandler().WriteToken(token),
                      userName = model.Username,
                      expiration = token.ValidTo
                  });
            }
            return Unauthorized();
        }

        // GET: api/Flights
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<Flight>>> GetFlights()
        {
            return await _context.Flights.ToListAsync();
        }

        // GET: api/Flights/5
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<Flight>> GetFlight(int id)
        {
            var flight = await _context.Flights.FindAsync(id);

            if (flight == null)
            {
                return NotFound();
            }

            return flight;
        }

        // PUT: api/Flights/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        [Authorize(Roles = "Editor")]
        public async Task<IActionResult> PutFlight(int id, Flight flight)
        {
            if (id != flight.FlightId)
            {
                return BadRequest();
            }

            _context.Entry(flight).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!FlightExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Flights
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost("create")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Flight>> PostFlight(Flight flight)
        {
            _context.Flights.Add(flight);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetFlight", new { id = flight.FlightId }, flight);
        }

        // DELETE: api/Flights/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteFlight(int id)
        {
            var flight = await _context.Flights.FindAsync(id);
            if (flight == null)
            {
                return NotFound();
            }

            _context.Flights.Remove(flight);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [AllowAnonymous]
        private bool FlightExists(int id)
        {
            return _context.Flights.Any(e => e.FlightId == id);
        }

        [HttpOptions]
        public IActionResult Options()
        {
            return Ok();
        }
    }
}
