# 🛠️ Teknik CRM & Service Management System (v1.0)

Bu proje, bir teknik servisin operasyonel süreçlerini dijitalleştiren, müşteri ilişkileri ile finansal takibi birleştiren **Full-Stack bir CRM uygulamasıdır.** Basit bir servis kayıt formunun ötesinde, işletme sahibinin karlılığını ve günlük iş akışını yönetebileceği bir mini-ERP mantığıyla geliştirilmiştir.

---

## 📸 Proje Ekran Görüntüleri (Showcase)

<div align="center">
  <h3>🛡️ Yönetim Paneli ve Veri Analizi</h3>
  <img src="img/dashboard.png" alt="Yönetim Paneli" width="48%" style="margin: 5px;" />
  <img src="img/finans.png" alt="Finans Takibi" width="48%" style="margin: 5px;" />
  <p><i>Sol: Servis Performans Grafikleri ve KPI Kartları | Sağ: Kasa Hareketleri ve Gelir-Gider Dengesi</i></p>
  
  <br />
  
  <h3>👤 Müşteri ve Cihaz Yönetimi</h3>
  <img src="img/detay.png" alt="Müşteri Profil Paneli" width="48%" style="margin: 5px;" />
  <img src="img/isemri.png" alt="İş Emirleri Listesi" width="48%" style="margin: 5px;" />
  <p><i>Sol: Cihaz Geçmişi ve Finans Özeti | Sağ: Dinamik İş Emri Takip Listesi</i></p>
</div>

---

## 🚀 Öne Çıkan Mühendislik Çözümleri

### 📊 Dinamik Veri Görselleştirme (Dashboard)
İşletme sahibinin sistem genel durumunu saniyeler içinde analiz edebilmesi için:
* **Performans Analizi:** Son 6 ayın servis kayıt yoğunluğu `Chart.js` kullanılarak line chart üzerinde asenkron olarak görselleştirilir.
* **Durum Dağılımı:** Aktif iş emirlerinin (İşlemde, Parça Bekliyor, Tamamlandı) yüzdesel dağılımı doughnut chart ile sunulur.

### 💰 Entegre Finans ve Kasa Modülü
Sistem, teknik işlemlerle finansal kayıtları birbirine sıkı sıkıya bağlar:
* **Otomatik Tahsilat Mantığı:** Bir servis kaydı kapatıldığında, belirlenen ücret otomatik olarak kasa hareketlerine "Tahsilat" olarak yansıtılır.
* **Net Nakit Akışı:** Gider kalemleri (parça alımı, kira vb.) kasadan düşülerek işletmenin anlık net kar durumu hesaplanır.
* **Excel Raporlama:** `ClosedXML` kütüphanesi kullanılarak tüm finansal veriler tek tıkla profesyonel Excel dökümlerine dönüştürülür.

### 📧 SMTP Akıllı Bildirim Sistemi
* **Borç Hatırlatma:** Ödemesi beklenen müşterilere, sistem üzerinden `MailKit` entegrasyonu ile HTML şablonlu borç hatırlatma mailleri gönderilir.
* **Müşteri İletişimi:** Servis süreçleri hakkında asenkron bilgilendirme altyapısı kurulmuştur.

### 🔍 Global Arama ve Detaylı Profilleme
* **360 Derece Müşteri Görünümü:** Bir müşteri seçildiğinde; o müşteriye ait tüm cihazlar, geçmiş servis kayıtları ve toplam ciro bilgisi modal üzerinden (AJAX) dinamik olarak yüklenir.

---

## 🛠️ Teknik Yetkinlikler (Tech Stack)

* **Backend:** ASP.NET Core MVC 8.0 (C#)
* **Veritabanı:** Microsoft SQL Server & T-SQL
* **ORM:** Entity Framework Core (Code-First & Database-First Mix)
* **Frontend:** Bootstrap 5, JQuery (AJAX), Chart.js, SweetAlert2, DataTables.
* **Güvenlik:** Role-based Authorization (Admin/Teknisyen), Anti-Forgery Token koruması, Session Management.

---

## 🏗️ Veritabanı Mimarisi

Sistem, ilişkisel veritabanı kurallarına (Normalization) uygun 12 modülden oluşmaktadır:
* `Musteriler`: Bireysel ve kurumsal müşteri verileri.
* `ServisKayitlari`: Cihaz arıza detayları, durum takibi ve ücretlendirme.
* `KasaHareketleri`: İşletmenin tüm gelir/gider logları.
* `Randevular`: Tarih ve saat bazlı asenkron ajanda yönetimi.

---

## 🚧 Roadmap (Gelecek Planları)

- [ ] **📱 Mobil Teknisyen Uygulaması:** Sahadaki teknisyenler için PWA desteği.
- [ ] **💬 WhatsApp Entegrasyonu:** Servis durumu güncellendiğinde otomatik WhatsApp mesajı.
- [ ] **📦 Stok Yönetimi:** Kullanılan yedek parçaların otomatik stoktan düşülmesi.

---

## 🔧 Kurulum ve Çalıştırma

1. Projeyi klonlayın.
2. SQL Server üzerinde `CrmDb` adında bir veritabanı oluşturun.
3. Proje ana dizinindeki **`CRM_DB.sql`** dosyasını SSMS üzerinden bu veritabanında çalıştırın.
4. `appsettings.json` dosyasındaki `ConnectionStrings` alanını yerel server bilgilerinize göre güncelleyin.
5. Visual Studio üzerinden projeyi derleyip (Build) çalıştırın.

---
*Bu proje, **Beyza Akbulut** tarafından modern yazılım geliştirme standartları gözetilerek bir portfolyo çalışması olarak geliştirilmiştir.*
