using System.ComponentModel.DataAnnotations.Schema;

namespace vega.Migrations.DAL
{
    [Table("geometry_types")]
    public class GeometryType
    {
        [Column("type_id")]
        public int Id { get; set; }
        [Column("geometry_type")]
        public string Name { get; set; }
    }
}
