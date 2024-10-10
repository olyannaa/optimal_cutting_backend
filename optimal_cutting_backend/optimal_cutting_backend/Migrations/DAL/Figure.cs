using System.ComponentModel.DataAnnotations.Schema;

namespace vega.Migrations.DAL
{
    [Table("figures")]
    public class Figure
    {
        [Column("figures_id")]
        public int Id { get; set; }
        [Column("filename_id")]
        public int FilenameId { get; set; }
        [Column("type_id")]
        public int TypeId { get; set; }
        [Column("coordinates")]
        public string Coordinates{ get; set; }
    }
}
