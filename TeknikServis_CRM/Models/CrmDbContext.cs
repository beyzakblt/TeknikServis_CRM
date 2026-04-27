using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace TeknikServis_CRM.Models;

public partial class CrmDbContext : DbContext
{
    public CrmDbContext()
    {
    }

    public CrmDbContext(DbContextOptions<CrmDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AktiviteLoglari> AktiviteLoglaris { get; set; }

    public virtual DbSet<Bildirimler> Bildirimlers { get; set; }

    public virtual DbSet<CihazTipleri> CihazTipleris { get; set; }

    public virtual DbSet<Cihazlar> Cihazlars { get; set; }

    public virtual DbSet<IsEmirleri> IsEmirleris { get; set; }

    public virtual DbSet<KasaHareketleri> KasaHareketleris { get; set; }

    public virtual DbSet<KullaniciOturumlari> KullaniciOturumlaris { get; set; }

    public virtual DbSet<KullaniciRolleri> KullaniciRolleris { get; set; }

    public virtual DbSet<Kullanicilar> Kullanicilars { get; set; }

    public virtual DbSet<MusteriAdresleri> MusteriAdresleris { get; set; }

    public virtual DbSet<MusteriAktiviteLoglari> MusteriAktiviteLoglaris { get; set; }

    public virtual DbSet<MusteriBelgeleri> MusteriBelgeleris { get; set; }

    public virtual DbSet<MusteriCihazlari> MusteriCihazlaris { get; set; }

    public virtual DbSet<MusteriDurumlari> MusteriDurumlaris { get; set; }

    public virtual DbSet<MusteriEtiketAtamalari> MusteriEtiketAtamalaris { get; set; }

    public virtual DbSet<MusteriEtiketleri> MusteriEtiketleris { get; set; }

    public virtual DbSet<MusteriNotTipleri> MusteriNotTipleris { get; set; }

    public virtual DbSet<MusteriNotlari> MusteriNotlaris { get; set; }

    public virtual DbSet<MusteriTipleri> MusteriTipleris { get; set; }

    public virtual DbSet<Musteriler> Musterilers { get; set; }

    public virtual DbSet<Randevular> Randevulars { get; set; }

    public virtual DbSet<RolYetkileri> RolYetkileris { get; set; }

    public virtual DbSet<Roller> Rollers { get; set; }

    public virtual DbSet<ServisDurumlari> ServisDurumlaris { get; set; }

    public virtual DbSet<ServisKayitlari> ServisKayitlaris { get; set; }

    public virtual DbSet<SifreSifirlamaTalepleri> SifreSifirlamaTalepleris { get; set; }

    public virtual DbSet<SistemAyarlari> SistemAyarlaris { get; set; }

    public virtual DbSet<TahsilatHareketleri> TahsilatHareketleris { get; set; }

    public virtual DbSet<Yetkiler> Yetkilers { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=beyza;Database=CRM_DB;Trusted_Connection=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AktiviteLoglari>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Aktivite__3214EC073F446352");

            entity.ToTable("AktiviteLoglari");

            entity.HasIndex(e => e.KullaniciId, "IX_AktiviteLoglari_KullaniciId");

            entity.Property(e => e.IslemTipi).HasMaxLength(100);
            entity.Property(e => e.ModulAdi).HasMaxLength(100);
            entity.Property(e => e.Tarih)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
        });

        modelBuilder.Entity<Bildirimler>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Bildirim__3214EC07F71E4BA3");

            entity.ToTable("Bildirimler");

            entity.Property(e => e.OkunduMu).HasDefaultValue(false);
            entity.Property(e => e.Tarih)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
        });

        modelBuilder.Entity<CihazTipleri>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__CihazTip__3214EC077425CC00");

            entity.ToTable("CihazTipleri");

            entity.Property(e => e.TipAdi).HasMaxLength(100);
            entity.Property(e => e.TipKodu).HasMaxLength(100);
        });

        modelBuilder.Entity<Cihazlar>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.ToTable("Cihazlar");

            entity.Property(e => e.Marka).HasMaxLength(50);
            entity.Property(e => e.Model).HasMaxLength(50);
            entity.Property(e => e.SeriNo).HasMaxLength(100);

            // MusteriId varsa kalsın, yoksa bunu da yorum satırı yapın
            entity.Property(e => e.MusteriId).HasColumnName("MusteriId");

            // BURADAKİ CihazTipId İLE İLGİLİ TÜM SATIRLARI SİLDİK
        });

        modelBuilder.Entity<IsEmirleri>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__IsEmirle__3214EC0783E14198");

            entity.ToTable("IsEmirleri");

            entity.Property(e => e.Durum).HasMaxLength(50);
            entity.Property(e => e.OlusturmaTarihi)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ServisUcreti).HasColumnType("decimal(18, 2)");
        });

        modelBuilder.Entity<KasaHareketleri>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__KasaHare__3214EC07C1C751EE");

            entity.ToTable("KasaHareketleri");

            entity.Property(e => e.IslemTipi).HasMaxLength(20);
            entity.Property(e => e.OdemeYontemi).HasMaxLength(50);
            entity.Property(e => e.Tarih)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Tutar).HasColumnType("decimal(18, 2)");
        });

        modelBuilder.Entity<KullaniciOturumlari>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Kullanic__3214EC071DB9123B");

            entity.ToTable("KullaniciOturumlari");

            entity.Property(e => e.AktifMi).HasDefaultValue(true);
            entity.Property(e => e.GirisTarihi)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IpAdresi).HasMaxLength(50);
            entity.Property(e => e.Token).HasMaxLength(500);
        });

        modelBuilder.Entity<KullaniciRolleri>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Kullanic__3214EC079B948093");

            entity.ToTable("KullaniciRolleri");

            entity.HasIndex(e => e.KullaniciId, "IX_KullaniciRolleri_KullaniciId");

            entity.HasIndex(e => new { e.KullaniciId, e.RolId }, "UQ_KullaniciRolleri_KullaniciRol").IsUnique();

            entity.Property(e => e.AtanmaTarihi)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
        });

        modelBuilder.Entity<Kullanicilar>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Kullanic__3214EC07E404263A");

            entity.ToTable("Kullanicilar");

            entity.HasIndex(e => e.Eposta, "UQ_Kullanicilar_Eposta").IsUnique();

            entity.HasIndex(e => e.KullaniciAdi, "UQ_Kullanicilar_KullaniciAdi").IsUnique();

            entity.Property(e => e.Ad).HasMaxLength(100);
            entity.Property(e => e.AdSoyad).HasMaxLength(250);
            entity.Property(e => e.Durum)
                .HasMaxLength(20)
                .HasDefaultValue("Aktif");
            entity.Property(e => e.Eposta).HasMaxLength(200);
            entity.Property(e => e.IsOnayli).HasDefaultValue(false);
            entity.Property(e => e.KullaniciAdi).HasMaxLength(100);
            entity.Property(e => e.OlusturmaTarihi)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ProfilResmi).HasMaxLength(500);
            entity.Property(e => e.SifreHash).HasMaxLength(500);
            entity.Property(e => e.SonGirisTarihi).HasColumnType("datetime");
            entity.Property(e => e.Soyad).HasMaxLength(100);
            entity.Property(e => e.Telefon).HasMaxLength(30);
        });

        modelBuilder.Entity<MusteriAdresleri>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__MusteriA__3214EC071871C3FE");

            entity.ToTable("MusteriAdresleri");

            entity.Property(e => e.AcikAdres).HasMaxLength(500);
            entity.Property(e => e.AdresTarifi).HasMaxLength(500);
            entity.Property(e => e.AdresTipi)
                .HasMaxLength(50)
                .HasDefaultValue("AnaAdres");
            entity.Property(e => e.Durum)
                .HasMaxLength(20)
                .HasDefaultValue("Aktif");
            entity.Property(e => e.Il).HasMaxLength(100);
            entity.Property(e => e.Ilce).HasMaxLength(100);
            entity.Property(e => e.KonumNotu).HasMaxLength(300);
            entity.Property(e => e.Mahalle).HasMaxLength(150);
            entity.Property(e => e.OlusturmaTarihi)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.VarsayilanMi).HasDefaultValue(true);
        });

        modelBuilder.Entity<MusteriAktiviteLoglari>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__MusteriA__3214EC071A737CD5");

            entity.ToTable("MusteriAktiviteLoglari");

            entity.Property(e => e.Aciklama).HasMaxLength(1000);
            entity.Property(e => e.IslemTipi).HasMaxLength(100);
            entity.Property(e => e.Tarih)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
        });

        modelBuilder.Entity<MusteriBelgeleri>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__MusteriB__3214EC072519879C");

            entity.ToTable("MusteriBelgeleri");

            entity.Property(e => e.BelgeAdi).HasMaxLength(200);
            entity.Property(e => e.DosyaYolu).HasMaxLength(500);
            entity.Property(e => e.YuklemeTarihi)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
        });

        modelBuilder.Entity<MusteriCihazlari>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__MusteriC__3214EC079A8ABEB7");

            entity.ToTable("MusteriCihazlari");

            entity.HasIndex(e => e.MusteriId, "IX_MusteriCihazlari_MusteriId");

            entity.Property(e => e.AktifMi).HasDefaultValue(true);
            entity.Property(e => e.GarantiDurumu).HasMaxLength(50);
            entity.Property(e => e.KurulumTarihi).HasColumnType("datetime");
            entity.Property(e => e.Marka).HasMaxLength(100);
            entity.Property(e => e.Model).HasMaxLength(150);
            entity.Property(e => e.SeriNo).HasMaxLength(100);
        });

        modelBuilder.Entity<MusteriDurumlari>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__MusteriD__3214EC075613FED1");

            entity.ToTable("MusteriDurumlari");

            entity.Property(e => e.Aciklama).HasMaxLength(250);
            entity.Property(e => e.Durum)
                .HasMaxLength(20)
                .HasDefaultValue("Aktif");
            entity.Property(e => e.DurumAdi).HasMaxLength(50);
            entity.Property(e => e.DurumKodu).HasMaxLength(50);
            entity.Property(e => e.RenkKodu).HasMaxLength(20);
        });

        modelBuilder.Entity<MusteriEtiketAtamalari>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__MusteriE__3214EC07901A3726");

            entity.ToTable("MusteriEtiketAtamalari");

            entity.Property(e => e.AtanmaTarihi)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
        });

        modelBuilder.Entity<MusteriEtiketleri>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__MusteriE__3214EC0755B9DCC1");

            entity.ToTable("MusteriEtiketleri");

            entity.Property(e => e.Durum)
                .HasMaxLength(20)
                .HasDefaultValue("Aktif");
            entity.Property(e => e.EtiketAdi).HasMaxLength(100);
            entity.Property(e => e.EtiketKodu).HasMaxLength(100);
            entity.Property(e => e.RenkKodu).HasMaxLength(20);
        });

        modelBuilder.Entity<MusteriNotTipleri>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__MusteriN__3214EC073D0EAAC6");

            entity.ToTable("MusteriNotTipleri");

            entity.Property(e => e.NotTipiAdi).HasMaxLength(100);
            entity.Property(e => e.NotTipiKodu).HasMaxLength(100);
        });

        modelBuilder.Entity<MusteriNotlari>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__MusteriN__3214EC073E57DFFE");

            entity.ToTable("MusteriNotlari");

            entity.Property(e => e.Baslik).HasMaxLength(200);
            entity.Property(e => e.GizliMi).HasDefaultValue(false);
            entity.Property(e => e.Tarih)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
        });

        modelBuilder.Entity<MusteriTipleri>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__MusteriT__3214EC0769620794");

            entity.ToTable("MusteriTipleri");

            entity.Property(e => e.Aciklama).HasMaxLength(250);
            entity.Property(e => e.Durum)
                .HasMaxLength(20)
                .HasDefaultValue("Aktif");
            entity.Property(e => e.TipAdi).HasMaxLength(50);
            entity.Property(e => e.TipKodu).HasMaxLength(50);
        });

        modelBuilder.Entity<Musteriler>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Musteril__3214EC071B8D264C");

            entity.ToTable("Musteriler");

            entity.HasIndex(e => e.AdSoyad, "IX_Musteriler_AdSoyad");

            entity.HasIndex(e => e.Telefon, "IX_Musteriler_Telefon");

            entity.Property(e => e.Aciklama).HasMaxLength(500);
            entity.Property(e => e.Ad).HasMaxLength(100);
            entity.Property(e => e.AdSoyad).HasMaxLength(250);
            entity.Property(e => e.Eposta).HasMaxLength(200);
            entity.Property(e => e.FirmaAdi).HasMaxLength(250);
            entity.Property(e => e.FirmaUnvani).HasMaxLength(250);
            entity.Property(e => e.GuncellemeTarihi).HasColumnType("datetime");
            entity.Property(e => e.IkinciTelefon).HasMaxLength(30);
            entity.Property(e => e.KaynakBilgisi).HasMaxLength(100);
            entity.Property(e => e.OlusturmaTarihi)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.PasifeAlinmaNedeni).HasMaxLength(300);
            entity.Property(e => e.PasifeAlinmaTarihi).HasColumnType("datetime");
            entity.Property(e => e.SabitTelefon).HasMaxLength(30);
            entity.Property(e => e.SilindiMi).HasDefaultValue(false);
            entity.Property(e => e.SonServisTarihi).HasColumnType("datetime");
            entity.Property(e => e.Soyad).HasMaxLength(100);
            entity.Property(e => e.Telefon).HasMaxLength(30);
            entity.Property(e => e.VergiDairesi).HasMaxLength(150);
            entity.Property(e => e.VergiNo).HasMaxLength(50);
            entity.Property(e => e.YetkiliKisi).HasMaxLength(200);
        });

        modelBuilder.Entity<Randevular>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Randevul__3214EC07F3698AD8");

            entity.ToTable("Randevular");

            entity.Property(e => e.Aciklama).HasMaxLength(500);
            entity.Property(e => e.RandevuTarihi).HasColumnType("datetime");
        });

        modelBuilder.Entity<RolYetkileri>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__RolYetki__3214EC070EB1BB7A");

            entity.ToTable("RolYetkileri");

            entity.HasIndex(e => e.RolId, "IX_RolYetkileri_RolId");
        });

        modelBuilder.Entity<Roller>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Roller__3214EC07E23D0194");

            entity.ToTable("Roller");

            entity.HasIndex(e => e.RolKodu, "UQ_Roller_RolKodu").IsUnique();

            entity.Property(e => e.Durum)
                .HasMaxLength(20)
                .HasDefaultValue("Aktif");
            entity.Property(e => e.RolAdi).HasMaxLength(100);
            entity.Property(e => e.RolKodu).HasMaxLength(100);
        });

        modelBuilder.Entity<ServisDurumlari>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ServisDu__3214EC0783E3468E");

            entity.ToTable("ServisDurumlari");

            entity.Property(e => e.DurumAdi).HasMaxLength(100);
            entity.Property(e => e.DurumKodu).HasMaxLength(100);
            entity.Property(e => e.RenkKodu).HasMaxLength(20);
        });

        modelBuilder.Entity<ServisKayitlari>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ServisKa__3214EC07068439A6");

            entity.ToTable("ServisKayitlari");

            entity.HasIndex(e => e.MusteriId, "IX_ServisKayitlari_MusteriId");

            entity.Property(e => e.ArizaAciklamasi).HasMaxLength(1000);
            entity.Property(e => e.KapanisTarihi).HasColumnType("datetime");
            entity.Property(e => e.OdemeDurumu)
                .HasMaxLength(50)
                .HasDefaultValue("Bekliyor");
            entity.Property(e => e.ServisNo).HasMaxLength(50);
            entity.Property(e => e.ServisTarihi)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Ucret)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(18, 2)");
            entity.Property(e => e.YapilanIslem).HasMaxLength(1000);
        });

        modelBuilder.Entity<SifreSifirlamaTalepleri>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__SifreSif__3214EC07907FE9CB");

            entity.ToTable("SifreSifirlamaTalepleri");

            entity.Property(e => e.Kod).HasMaxLength(200);
            entity.Property(e => e.SonKullanimTarihi).HasColumnType("datetime");
        });

        modelBuilder.Entity<SistemAyarlari>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__SistemAy__3214EC0784C6DA09");

            entity.ToTable("SistemAyarlari");

            entity.Property(e => e.AyarAnahtari).HasMaxLength(100);
        });

        modelBuilder.Entity<TahsilatHareketleri>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Tahsilat__3214EC07C7954CE6");

            entity.ToTable("TahsilatHareketleri");

            entity.Property(e => e.HareketTipi).HasMaxLength(50);
            entity.Property(e => e.IslemTarihi)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.OdemeYontemi).HasMaxLength(50);
            entity.Property(e => e.Tutar).HasColumnType("decimal(18, 2)");
        });

        modelBuilder.Entity<Yetkiler>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Yetkiler__3214EC07C6DDF8FC");

            entity.ToTable("Yetkiler");

            entity.Property(e => e.Durum)
                .HasMaxLength(20)
                .HasDefaultValue("Aktif");
            entity.Property(e => e.ModulAdi).HasMaxLength(100);
            entity.Property(e => e.YetkiAdi).HasMaxLength(150);
            entity.Property(e => e.YetkiKodu).HasMaxLength(150);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
