using System.ComponentModel.DataAnnotations;

namespace Eva2Auth.Model
{
    public class User
    {
        public Guid Id { get; set; }
        [Required]
        public string Username { get; set; }
        [Required]
        public byte[] PasswordHash { get; set; }
        [Required]
        public byte[] PasswordSalt { get; set; }
        [Required]
        public string Role { get; set; }
        [Required]
        public Boolean Enabled { get; set; }
        [Required]
        public string Email { get; set; }
    }

    public class UserDTO
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }
        public string Email { get; set; }

    }

    public class UserDTOUpdate 
    {
    public string Username { get; set; }
    public string Role { get; set; }
    public string Email { get; set; }
}
}
