using System.ComponentModel.DataAnnotations;

namespace DemoJWT.Model
{
    public class AppUser
    {
        [Key]
        public int IdUser { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Salt { get; set; }
        public string RefreshToken { get; set; }
        public DateTime? RefreshTokenExp { get; set; }
    }
}
