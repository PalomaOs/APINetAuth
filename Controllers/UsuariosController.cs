using backendnet.Models;
using backendnet.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

namespace backendnet.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Administrador")]

public class UsuariosController(IdentityContext context, UserManager<CustomIdentityUser> userManager) : Controller
{
    // GET: api/usuarios
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CustomIdentityUserDTO>>> GetUsuarios()
    {
        var usuarios = new List<CustomIdentityUserDTO>();

        foreach (var usuario in await context.Users.AsNoTracking().ToListAsync())
        {
            usuarios.Add(new CustomIdentityUserDTO
            {
                Id = usuario.Id,
                Nombre = usuario.Nombre,
                Email = usuario.Email!,
                Rol = GetUserRole(usuario)
            });
        }
        return usuarios;
    }

    // GET: api/usuarios/email
    [HttpGet("{email}")]
    public async Task<ActionResult<CustomIdentityUserDTO>> GetUsuario(string email)
    {
        var usuario = await userManager.FindByEmailAsync(email);

        if (usuario == null) return NotFound();

        return new CustomIdentityUserDTO
        {
            Id = usuario.Id,
            Nombre = usuario.Nombre,
            Email = usuario.Email!,
            Rol = GetUserRole(usuario)
        };
    }

    // POST: api/usuarios
    [HttpPost]
    public async Task<ActionResult<CustomIdentityUserDTO>> PostUsuario(CustomIdentityUserPwdDTO usuarioDTO)
    {
        var usuarioToCreate = new CustomIdentityUser
        {
            UserName = usuarioDTO.Email,
            Email = usuarioDTO.Email,
            NormalizedEmail = usuarioDTO.Email.ToUpper(),
            Nombre = usuarioDTO.Nombre,
            NormalizedUserName = usuarioDTO.Email.ToUpper()
        };

        IdentityResult result = await userManager.CreateAsync(usuarioToCreate, usuarioDTO.Password);
        if (!result.Succeeded) return BadRequest(new { message = "Error al crear el usuario" });

        result = await userManager.AddToRoleAsync(usuarioToCreate, usuarioDTO.Rol);

        var usuarioViewModel = new CustomIdentityUserDTO
        {
            Id = usuarioToCreate.Id,
            Nombre = usuarioDTO.Nombre,
            Email = usuarioDTO.Email,
            Rol = usuarioDTO.Rol
        };
        return CreatedAtAction(nameof(GetUsuario), new { email = usuarioToCreate.Email }, usuarioViewModel);
    }

    // PUT: api/usuarios/email
    [HttpPut("{email}")]
    public async Task<IActionResult> PutUsuario(string email, CustomIdentityUserDTO usuarioDTO)
    {
        if (email != usuarioDTO.Email) return BadRequest();

        var usuario = await userManager.FindByEmailAsync(email);

        if (usuario == null) return NotFound();

        if (await context.Roles.Where(r => r.Name == usuarioDTO.Rol).FirstOrDefaultAsync() == null) return NotFound();

        usuario.Nombre = usuarioDTO.Nombre;
        usuario.NormalizedUserName = usuarioDTO.Email.ToUpper();
        IdentityResult result = await userManager.UpdateAsync(usuario);
        if (!result.Succeeded) return BadRequest();

        foreach (var rol in await context.Roles.ToListAsync())
            await userManager.RemoveFromRoleAsync(usuario, rol.Name!);
        await userManager.AddToRoleAsync(usuario, usuarioDTO.Rol);

        return NoContent();
    }

    // DELETE: api/usuarios/email
    [HttpDelete("{email}")]
    public async Task<IActionResult> DeleteUsuario(string email)
    {
        var usuario = await userManager.FindByEmailAsync(email);

        if (usuario == null) return NotFound();

        if (usuario.Protegido) return StatusCode(StatusCodes.Status403Forbidden);

        IdentityResult result = await userManager.DeleteAsync(usuario);
        if (!result.Succeeded) return BadRequest();

        return NoContent();
    }

    private string GetUserRole(CustomIdentityUser usuario)
    {
        var roles = userManager.GetRolesAsync(usuario).Result;
        return roles.First();
    }
}
