using System.Security.Claims;
using backendnet.Services;
using backendnet.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace backendnet.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController(UserManager<CustomIdentityUser> userManager, JwtTokenService jwtTokenService) : Controller
{
    // POST: api/auth
    [HttpPost]
    public async Task<IActionResult> PostAsync([FromBody] LoginDTO loginDTO)
    {
        var usuario = await userManager.FindByEmailAsync(loginDTO.Email);

        if (usuario is null || !await userManager.CheckPasswordAsync(usuario, loginDTO.Password))
            return Unauthorized(new { message = "Usuario o contraseñas incorrectos" });

        var roles = await userManager.GetRolesAsync(usuario);
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, usuario.Email!),
            new(ClaimTypes.GivenName, usuario.Nombre),
            new(ClaimTypes.Role, roles.First()),
        };

        var jwt = jwtTokenService.GenerarToken(claims);

        return Ok(new
        {
            usuario.Email,
            usuario.Nombre,
            rol = string.Join(",", roles),
            jwt
        });
    }

    // GET: api/auth/tiempo
    [Authorize]
    [HttpGet("tiempo")]
    public IActionResult GetTiempo()
    {
        string? tiempo = jwtTokenService.TiempoRestanteToken();
        if (tiempo is null)
            return BadRequest();
        return Ok(new { tiempo });
    }
}