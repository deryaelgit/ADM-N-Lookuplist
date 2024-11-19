using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Portal.Models;
using Portal.ViewModels;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Portal.Helpers;
using Portal.Models.Admin;
using Portal.ViewModels.Admin;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Portal.Controllers.Admin
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ProfileController> _logger;

        public ProfileController(ApplicationDbContext context, ILogger<ProfileController> logger)
        {
            _context = context;
            _logger = logger;
        }
        public async Task<IActionResult> Index()
        {
            _logger.LogInformation("Mevcut kullanıcı alınıyor...");
            var email = User.Identity.Name;

            var user = await _context.Users
                .Include(u => u.KanGrubu)
                .Include(u => u.Sube)
                    .ThenInclude(s => s.Mudurluk)
                        .ThenInclude(m => m.Birim)
                .SingleOrDefaultAsync(u => u.Email == email);

            if (user == null)
            {
                _logger.LogWarning("Kullanıcı bulunamadı, giriş sayfasına yönlendiriliyor.");
                return RedirectToAction("Login", "Account");
            }

            // LookUpList'ten dinamik veriler
            ViewBag.Cinsiyetler = new SelectList(
                _context.LookUpLists.Where(l => l.LookUpId == 1).ToList(),
                "Id",
                "Name"
            );

            ViewBag.OgrenimDurumlari = new SelectList(
                _context.LookUpLists.Where(l => l.LookUpId == 4).ToList(),
                "Id",
                "Name"
            );

            ViewBag.MedeniDurumlar = new SelectList(
                _context.LookUpLists.Where(l => l.LookUpId == 5).ToList(),
                "Id",
                "Name"
            );

            ViewBag.KanGruplari = new SelectList(
                _context.LookUpLists.Where(l => l.LookUpId == 2).ToList(),
                "Id",
                "Name"
            );

            ViewBag.Ehliyetler = new SelectList(
                _context.LookUpLists.Where(l => l.LookUpId == 3).ToList(),
                "Id",
                "Name"
            );

            // Kullanıcı bilgilerini ProfileViewModel'e dönüştür
            var model = new ProfileViewModel
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Company = user.Company,
                JobTitle = user.JobTitle,
                Country = user.Country,
                Address = user.Address,
                Phone = user.Phone,
                Twitter = user.Twitter,
                Facebook = user.Facebook,
                Instagram = user.Instagram,
                Puan = user.Puan,
                ProfileImage = user.ProfileImage,
                About = user.About,
                Password = user.Password,
                TcKimlik = user.TcKimlik,
                BirthDate = user.BirthDate,
                CinsiyetId = user.CinsiyetId,
                OgrenimDurumuId = user.OgrenimDurumuId,
                MedeniDurumId = user.MedeniDurumId,
                KanGrubuId = user.KanGrubuId,
                EhliyetId = user.EhliyetId,
                BirimAd = user.Sube?.Mudurluk?.Birim?.Ad,
                MudurlukAd = user.Sube?.Mudurluk?.Ad,
                SubeAd = user.Sube?.Ad,
                KanGrubu = user.KanGrubuId.HasValue
                  ? _context.LookUpLists.FirstOrDefault(l => l.Id == user.KanGrubuId)?.Name
                  : null,
                Cinsiyet = user.CinsiyetId.HasValue
                  ? _context.LookUpLists.FirstOrDefault(l => l.Id == user.CinsiyetId)?.Name
                  : null,
                OgrenimDurumu = user.OgrenimDurumuId.HasValue
                  ? _context.LookUpLists.FirstOrDefault(l => l.Id == user.OgrenimDurumuId)?.Name
                  : null,
                MedeniDurum = user.MedeniDurumId.HasValue
                  ? _context.LookUpLists.FirstOrDefault(l => l.Id == user.MedeniDurumId)?.Name
                  : null,
                Ehliyet = user.EhliyetId.HasValue
                  ? _context.LookUpLists.FirstOrDefault(l => l.Id == user.EhliyetId)?.Name
                  : null
            };


            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Update(ProfileViewModel model)
        {
            _logger.LogInformation("Güncelleme işlemi başlatıldı... ModelState geçerli: {ModelStateValid}", ModelState.IsValid);

            if (ModelState.IsValid)
            {
                _logger.LogInformation("Kullanıcı ID: {UserId}", model.Id);

                var user = await _context.Users.FindAsync(model.Id);
                if (user == null)
                {
                    _logger.LogWarning("Güncellenecek kullanıcı bulunamadı. Kullanıcı ID: {UserId}", model.Id);
                    return NotFound();
                }

                _logger.LogInformation("Kullanıcı bulundu: {UserId}", user.Id);

                // Kullanıcı bilgilerini güncelle
                user.FullName = model.FullName;
                user.Email = model.Email;
                user.Password = model.Password; // Şifreyi hashlemeniz gerekiyorsa burada hashleyin
                user.About = model.About ?? string.Empty;
                user.Address = model.Address ?? string.Empty;
                user.Company = model.Company ?? string.Empty;
                user.JobTitle = model.JobTitle ?? string.Empty;
                user.Country = model.Country ?? string.Empty;
                user.Phone = model.Phone;
                user.Twitter = model.Twitter ?? string.Empty;
                user.Facebook = model.Facebook ?? string.Empty;
                user.Instagram = model.Instagram ?? string.Empty;
                user.Puan = model.Puan ?? string.Empty;
                user.TcKimlik = model.TcKimlik;
                user.BirthDate = model.BirthDate;

                // LookUpList'ten gelen Id'ler ile güncelleme
                user.CinsiyetId = model.CinsiyetId; // Cinsiyet Id
                user.OgrenimDurumuId = model.OgrenimDurumuId; // Öğrenim Durumu Id
                user.KanGrubuId = model.KanGrubuId; // Kan Grubu Id
                user.MedeniDurumId = model.MedeniDurumId; // Medeni Durum Id
                user.EhliyetId = model.EhliyetId; // Ehliyet Id

                _logger.LogInformation("Kullanıcı bilgileri güncellendi: {UserId}", user.Id);

                // Profil resmi güncelleniyorsa işle
                if (model.ProfileImageFile != null)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await model.ProfileImageFile.CopyToAsync(memoryStream);
                        user.ProfileImage = memoryStream.ToArray();
                    }
                    _logger.LogInformation("Profil resmi güncellendi: {UserId}", user.Id);
                }

                // Veritabanı güncellemesi
                _context.Update(user);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Veritabanına kaydedildi: {UserId}", user.Id);

                return RedirectToAction("Index");
            }

            // ModelState geçersizse hataları logla
            _logger.LogWarning("ModelState geçersiz. Güncelleme işlemi başarısız. Hatalar: {ModelStateErrors}",
                string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));

            // LookUpList verilerini yeniden yükle
            ViewBag.Cinsiyetler = new SelectList(
                _context.LookUpLists.Where(l => l.LookUpId == 1).ToList(),
                "Id",
                "Name"
            );

            ViewBag.OgrenimDurumlari = new SelectList(
                _context.LookUpLists.Where(l => l.LookUpId == 4).ToList(),
                "Id",
                "Name"
            );

            ViewBag.MedeniDurumlar = new SelectList(
                _context.LookUpLists.Where(l => l.LookUpId == 5).ToList(),
                "Id",
                "Name"
            );

            ViewBag.KanGruplari = new SelectList(
                _context.LookUpLists.Where(l => l.LookUpId == 2).ToList(),
                "Id",
                "Name"
            );

            ViewBag.Ehliyetler = new SelectList(
                _context.LookUpLists.Where(l => l.LookUpId == 3).ToList(),
                "Id",
                "Name"
            );

            return View(model);
        }



        [HttpGet]
        public IActionResult ShowImage(int id)
        {
            var user = _context.Users.Find(id);
            if (user != null && user.ProfileImage != null)
            {
                return File(user.ProfileImage, "image/jpeg"); // veya image/png
            }
            return NotFound();
        }

        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!int.TryParse(userId, out var userIdAsInt))
                {
                    ModelState.AddModelError("", "Kullanıcı kimliği geçersiz.");
                    return View(model);
                }

                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userIdAsInt);

                if (user == null)
                {
                    ModelState.AddModelError("", "Kullanıcı bulunamadı.");
                    return View(model);
                }

                // Mevcut şifreyi hashleyip doğrulama işlemi
                if (!HashHelper.VerifyPassword(model.CurrentPassword, user.Password))
                {
                    ModelState.AddModelError("", "Mevcut şifre yanlış.");
                    return View(model);
                }

                // Yeni şifreyi hashleyip kullanıcıya atama işlemi
                user.Password = HashHelper.HashPassword(model.NewPassword);
                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                ViewBag.Message = "Şifre başarıyla değiştirildi.";


            }
            return View(model);
        }

    }


}