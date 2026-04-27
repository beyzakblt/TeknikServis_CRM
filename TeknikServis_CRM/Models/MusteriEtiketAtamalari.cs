using TeknikServis_CRM.Models;

public partial class MusteriEtiketAtamalari
{
    public int Id { get; set; }

    public int MusteriId { get; set; }

    public int EtiketId { get; set; }

    public DateTime? AtanmaTarihi { get; set; }

    // 🔥 EKLEMEN GEREKENLER
    public virtual Musteriler Musteri { get; set; } = null!;
    public virtual MusteriEtiketleri Etiket { get; set; } = null!;
}