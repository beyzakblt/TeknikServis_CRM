public partial class MusteriEtiketleri
{
    public int Id { get; set; }

    public string EtiketAdi { get; set; } = null!;

    public string EtiketKodu { get; set; } = null!;

    public string? RenkKodu { get; set; }

    public string? Durum { get; set; }

    // 🔥 EKLE
    public virtual ICollection<MusteriEtiketAtamalari> MusteriEtiketAtamalari { get; set; } = new List<MusteriEtiketAtamalari>();
}