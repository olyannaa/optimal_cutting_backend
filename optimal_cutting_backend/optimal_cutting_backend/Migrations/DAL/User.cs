using System.ComponentModel.DataAnnotations.Schema;

namespace vega.Migrations.DAL
{
    [Table("users")]
    public class User
    {
        [Column("user_id")]
        public int Id { get; set; }
        [Column("login")]
        public string Login { get; set; }
        [Column("full_name")]
        public string FullName { get; set; }
        [Column("password")]
        public string Password { get; set; }
    }
}
