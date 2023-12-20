using Eva2Auth.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "SuperAdministrador, Administrador")]

public class ManagementUserController : ControllerBase
{
    private readonly AppDbContext _context;

    public ManagementUserController(AppDbContext context)
    {
        _context = context;
    }

    [HttpDelete]
    [Route("Delete")]
    public async Task<IActionResult> Delete(string username)
    {
        var user = await _context.TblUsers.Where(u => u.Username == username).FirstOrDefaultAsync();
        if (user != null)
        {
            if (user.Role == "SuperAdministrador")
            {
                return BadRequest("Can't delete SuperAdministrador");
            }
            else
            {
                _context.TblUsers.Remove(user);
                await _context.SaveChangesAsync();
                return Ok();
            }
        }
        else { return BadRequest("User Not Found"); }
    }

    [HttpPatch]
    [Route("Block")]
    public async Task<IActionResult> Block(string username)
    {
        var user = await _context.TblUsers.Where(u => u.Username == username).FirstOrDefaultAsync();
        if (user != null)
        {
            if (user.Role == "SuperAdministrador")
            {
                return BadRequest("Can't Block SuperAdministrador");
            }
            else
            {
                user.Enabled = false;
                await _context.SaveChangesAsync();
                return Ok();
            }
        }
        else { return BadRequest("User Not Found"); }
    }

    [HttpPatch]
    [Route("Unlock")]
    public async Task<IActionResult> Unlock(string username)
    {
        var user = await _context.TblUsers.Where(u => u.Username == username).FirstOrDefaultAsync();
        if (user != null)
        {
            if (user.Role == "SuperAdministrador")
            {
                return BadRequest("Can't Unlock SuperAdministrador, because this account can´t block");
            }
            else
            {
                user.Enabled = true;
                await _context.SaveChangesAsync();
                return Ok();
            }
        }
        else { return BadRequest("User Not Found"); }
    }
    [HttpPatch]
    [Route("ToggleEnableAccount")]
    public async Task<IActionResult> ToggleEnableAccount(string username)
    {
        var user = await _context.TblUsers.Where(u => u.Username == username).FirstOrDefaultAsync();
        if (user != null)
        {
            if (user.Role == "SuperAdministrador")
            {
                return BadRequest("Can't Unlock o Block SuperAdministrador");
            }
            else
            {
                user.Enabled = !user.Enabled;
                await _context.SaveChangesAsync();
                return Ok();
            }
        }
        else { return BadRequest("User Not Found"); }
    }

    [HttpPatch]
    [Route("ChangePassword")]
    public async Task<IActionResult> ChangePassword(string username, string password)
    {
        var user = await _context.TblUsers.Where(u => u.Username == username).FirstOrDefaultAsync();
        if (user != null)
        {
            AuthController.CreatePasswordHash(password, out byte[] hash, out byte[] salt);
            user.PasswordHash = hash;
            user.PasswordSalt = salt;
            await _context.SaveChangesAsync();
            return Ok();
        }
        else { return BadRequest("User Not Found"); }
    }

    [HttpPut]
    [Route("Update")]
    public async Task<IActionResult> Update(UserDTOUpdate userDTO,string username)
    {
        var user = await _context.TblUsers.Where(u => u.Username == username).FirstOrDefaultAsync();
        if (user != null)
        {
            if (!AuthController.roles.Contains(userDTO.Role))
            {
                return BadRequest("Error in Role, please write a role correct");
            }
            else
            {
                user.Username = userDTO.Username;
                user.Role = userDTO.Role;
                user.Email = userDTO.Email;
                await _context.SaveChangesAsync();
                return Ok();
            }

        }
        else { return BadRequest("User Not Found"); }
    }
}
