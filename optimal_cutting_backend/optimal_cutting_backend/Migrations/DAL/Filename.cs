using System.ComponentModel.DataAnnotations.Schema;

namespace vega.Migrations.DAL
{
    [Table("filenames")]
    public class Filename
    {
        [Column("filename_id")]
        public int Id { get; set; }
        [Column("filename")]
        public string FileName { get; set; }
        [Column("oboznacenie")]
        public string Designation { get; set; }
        [Column("naimenovanie")]
        public string Name { get; set; }
        [Column("thickness")]
        public int Thickness { get; set; }
        [Column("user_id")]
        public int UserId { get; set; }
        [Column("material_id")]
        public int MaterialId { get; set; }

    }
}
