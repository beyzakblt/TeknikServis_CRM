using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TeknikServis_CRM.Models
{
    public partial class Cihazlar
    {
        [Key]
        public int Id { get; set; }

        public int? MusteriId { get; set; } // Bu olmazsa modelBuilder hata verir
        public int? CihazTipId { get; set; } // Bu olmazsa modelBuilder hata verir

        public string Marka { get; set; }
        public string Model { get; set; }
        public string SeriNo { get; set; }
        public string CihazNotu { get; set; }

        // Navigation Properties (İlişkiler)
        [ForeignKey("MusteriId")]
        public virtual Musteriler Musteri { get; set; }

        //[ForeignKey("CihazTipId")]
        //public virtual CihazTipleri CihazTip { get; set; }
    }
}