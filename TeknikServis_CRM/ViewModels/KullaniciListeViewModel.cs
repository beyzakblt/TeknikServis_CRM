namespace TeknikServis_CRM.Models
{
    public class KullaniciListeViewModel
    {
        // Kullanıcı Bilgileri
        public int Id { get; set; }
        public string Ad { get; set; } = null!;
        public string Soyad { get; set; } = null!;
        public string KullaniciAdi { get; set; } = null!;
        public string Eposta { get; set; } = null!;
        public string? Telefon { get; set; }
        public string Durum { get; set; } = null!; // Aktif/Pasif
        public DateTime? SonGiris { get; set; }

        // Rol Bilgileri (Birden fazla rolü olabilir diye liste tutuyoruz)
        public List<string> Roller { get; set; } = new List<string>();

        // Tek bir ana rol göstermek istersen (Örn: Admin)
        public string AnaRol => Roller.FirstOrDefault() ?? "Yetkisiz";
    }
}