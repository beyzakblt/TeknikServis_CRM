using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TeknikServis_CRM.Models
{
    [Table("ServisKayitlari")]
    public partial class ServisKayitlari
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string ServisNo { get; set; } = null!;

        [Column("MusteriId")]
        public int MusteriId { get; set; }

        [Column("MusteriCihazId")]
        public int? MusteriCihazId { get; set; }

        [Column("ServisDurumId")]
        public int ServisDurumId { get; set; }

        public string? ArizaAciklamasi { get; set; }
        public string? YapilanIslem { get; set; }
        public int? TeknisyenKullaniciId { get; set; }
        public decimal? Ucret { get; set; }
        public string? OdemeDurumu { get; set; }
        public DateTime? ServisTarihi { get; set; }

        // Veritabanında var olan bu sütunu "Ödeme Vadesi" olarak kullanıyoruz
        public DateTime? KapanisTarihi { get; set; }

        [ForeignKey("MusteriId")]
        public virtual Musteriler? Musteri { get; set; }

        [ForeignKey("MusteriCihazId")]
        public virtual MusteriCihazlari? MusteriCihaz { get; set; }

        [ForeignKey("ServisDurumId")]
        public virtual ServisDurumlari? ServisDurum { get; set; }
    }
}