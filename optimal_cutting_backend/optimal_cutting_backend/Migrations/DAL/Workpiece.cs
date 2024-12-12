using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace vega.Migrations.DAL
{
    [Table("workpieces")]
    public class Workpiece
    {
        [Key]
        [Column("workpiece_id")]
        public int Id { get; set; }
        [Column("name")]
        public string Name { get; set; }
        [Column("width")]
        public int Width { get; set; }
        [Column("height")]
        public int Height { get; set; }
    }
}
