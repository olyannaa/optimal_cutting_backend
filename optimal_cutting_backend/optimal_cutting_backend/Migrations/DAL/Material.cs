using System.ComponentModel.DataAnnotations.Schema;

namespace vega.Migrations.DAL
{
    [Table("material")]
    public class Material
    {
        [Column("material_id")]
        public int Id { get; set; }
        [Column("material")]
        public string Name { get; set; }
    }
}
