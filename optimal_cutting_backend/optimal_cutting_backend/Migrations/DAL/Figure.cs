using System.ComponentModel.DataAnnotations.Schema;

namespace vega.Migrations.DAL
{
    [Table("figures")]
    public class Figure
    {
        [Column("figures_id")]
        public int Id { get; set; }
        [Column("filename_id")]
        public int Filenameid { get; set; }
        [Column("type_id")]
        public int TypeId { get; set; }
        [Column("coorditanes")]
        public string Coordinates{ get; set; }
    }
}
