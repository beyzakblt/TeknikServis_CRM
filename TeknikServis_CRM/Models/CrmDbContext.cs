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

    public virtual DbSet<Cihazlar> Cihazlars { get; set; }

    public virtual DbSet<IsEmirleri> IsEmirleris { get; set; }

    public virtual DbSet<KasaHareketleri> KasaHareketleris { get; set; }

    public virtual DbSet<KullaniciOturumlari> KullaniciOturumlaris { get; set; }

    public virtual DbSet<KullaniciRolleri> KullaniciRolleris { get; set; }

    public virtual DbSet<Kullanicilar> Kullanicilars { get; set; }

    public virtual DbSet<Musteriler> Musterilers { get; set; }

    public virtual DbSet<Randevular> Randevulars { get; set; }

    public virtual DbSet<RolYetkileri> RolYetkileris { get; set; }

    public virtual DbSet<Roller> Rollers { get; set; }

    public virtual DbSet<SifreSifirlamaTalepleri> SifreSifirlamaTalepleris { get; set; }

    public virtual DbSet<SistemAyarlari> SistemAyarlaris { get; set; }

    public virtual DbSet<Yetkiler> Yetkilers { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Name=DB");

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

        modelBuilder.Entity<Cihazlar>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Cihazlar__3214EC072EF0A1EC");

            entity.ToTable("Cihazlar");

            entity.Property(e => e.Marka).HasMaxLength(50);
            entity.Property(e => e.Model).HasMaxLength(50);
            entity.Property(e => e.SeriNo).HasMaxLength(100);
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

        modelBuilder.Entity<Musteriler>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Musteril__3214EC07590FB88B");

            entity.ToTable("Musteriler");

            entity.Property(e => e.Ad).HasMaxLength(100);
            entity.Property(e => e.CariBakiye)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Eposta).HasMaxLength(100);
            entity.Property(e => e.Ilce).HasMaxLength(50);
            entity.Property(e => e.KayitTarihi)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Sehir).HasMaxLength(50);
            entity.Property(e => e.Soyad).HasMaxLength(100);
            entity.Property(e => e.Telefon).HasMaxLength(20);
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
