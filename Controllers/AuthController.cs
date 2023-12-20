using Eva2Auth.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _config;
    public static readonly List<string> roles = new List<string> {"Administrador", "Asistente", "Vendedor" };

    public AuthController(AppDbContext context, IConfiguration config)
    {
        _context = context;
        _config = config;
    }


    [HttpPost]
    [Route("Login")]
    public async Task<IActionResult> Login(string username, string password)
    {
        if (!_context.TblUsers.Any())
        {
            UserDTO userDTO = new UserDTO();
            userDTO.Username = username;
            //HARDCODEADO
            userDTO.Role = "SuperAdministrador";
            userDTO.Email = "Email@SuperAdmin.cl";
            //NO SE SETEA CON USERDTO DEBIDO A LOS REQUISITOS DE LA EVALAUCION
            userDTO.Password = "123456";
            return (await Register(userDTO, true));
        }
        else
        {
            var user = _context.TblUsers.Where(u => u.Username == username).FirstOrDefault();
            if (user != null)
            {
                if (!user.Enabled) return BadRequest("Blocked Account");

                if (VerifyPassword(password, user.PasswordHash, user.PasswordSalt))
                {
                    return Ok(CreateToken(user));
                }
                else
                {
                    return BadRequest("Password Incorrect");
                }
            }
            else
            {
                return BadRequest("User not found");
            }
        }
    }


    [HttpPost]
    [Route("Register")]
    [Authorize(Roles="SuperAdministrador, Administrador, Asistente, Vendedor")]
    public async Task<IActionResult> Register(UserDTO userDTO, bool newUser=false)
            
    {   
        //Evitar más de 1 SuperAdministrador
        //Se crea sin validaciones ya que será el primer usuario HARDCODEADO
        if (newUser)
        {
            User user = new User();
            user.Username = userDTO.Username;
            user.Role = userDTO.Role;
            user.Email = userDTO.Email;
            //HARDCODEADO
            user.Enabled = true;

            CreatePasswordHash(userDTO.Password, out byte[] hash, out byte[] salt);
            user.PasswordHash = hash;
            user.PasswordSalt = salt;

            _context.Add(user);
            await _context.SaveChangesAsync();
            return Ok(CreateToken(user));
        }
        else
        {
            if (userDTO.Role == "SuperAdministrador")
            {
                return BadRequest("There can only be one SuperAdministrado in the system");
            }
            else if (!roles.Contains(userDTO.Role))
            {
                return BadRequest("Error in Role, please write a role correct");
            }
            //EVITAR DUPLICIDAD DE NOMBRE
            else if (_context.TblUsers.Where(u => u.Username == userDTO.Username).Any())
            {
                return BadRequest("Username not available, please try another Username");
            }
            else
            {
                User user = new User();
                user.Username = userDTO.Username;
                user.Role = userDTO.Role;
                user.Email = userDTO.Email;
                //HARDCODEADO
                user.Enabled = true;

                CreatePasswordHash(userDTO.Password, out byte[] hash, out byte[] salt);
                user.PasswordHash = hash;
                user.PasswordSalt = salt;

                _context.Add(user);
                await _context.SaveChangesAsync();
                return Ok(CreateToken(user));
            }

        }
    }

    public static void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
    {
        using (var hmac = new HMACSHA512())
        {
            passwordSalt = hmac.Key;
            passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
        }
    }

    private bool VerifyPassword(string password, byte[] passwordHash, byte[] passwordSalt)
    {
        using (var hmac = new HMACSHA512(passwordSalt))
        {
            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            return computedHash.SequenceEqual(passwordHash);
        }
    }
    private string CreateToken(User user)
    {
        List<Claim> datos = new List<Claim>()
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role)
            };
        var Key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            _config.GetSection("AppSettings:TokenKey").Value));
        var Credential = new SigningCredentials(Key, SecurityAlgorithms.HmacSha512Signature);
        var Token = new JwtSecurityToken(
            claims: datos,
            expires: DateTime.UtcNow.AddDays(1),
            signingCredentials: Credential);
        var JWT = new JwtSecurityTokenHandler().WriteToken(Token);
        return JWT;
    }
}

