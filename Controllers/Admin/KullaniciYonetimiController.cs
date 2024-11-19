using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Portal.Models;
using Portal.Helpers;
using Portal.ViewModels;
using Portal.Controllers.Admin;
using Microsoft.EntityFrameworkCore;
using Portal.Models.Admin;
using Portal.ViewModels.Admin;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Portal.Controllers.Admin
{
    [Route("Admin/KullaniciYonetimi")]
    public class KullaniciYonetimiController : BaseController
    {
        private readonly ApplicationDbContext _context;

        public KullaniciYonetimiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ListUsers Action
        [HttpGet("ListUsers")]
        public IActionResult ListUsers()
        {
            var users = _context.Users
                .Include(u => u.Cinsiyet) // Cinsiyet bilgisini dahil et
                .Include(u => u.OgrenimDurumu) // Öğrenim durumunu dahil et
                .ToList();

            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            return View(users);
        }


        [HttpGet("AddUser")]
        public IActionResult AddUser()
        {
            // LookUpList tablosundan Cinsiyet (LookUpId = 1) verilerini al
            ViewBag.Cinsiyetler = new SelectList(
                _context.LookUpLists.Where(l => l.LookUpId == 1).ToList(),
                "Id",
                "Name"
            );

            // LookUpList tablosundan Cinsiyet (LookUpId = 1) verilerini al
            ViewBag.Ehliyetler = new SelectList(
                _context.LookUpLists.Where(l => l.LookUpId == 3).ToList(),
                "Id",
                "Name"
            );

            // LookUpList tablosundan Medeni Durum (LookUpId = 2) verilerini al
            ViewBag.MedeniDurumlar = new SelectList(
                _context.LookUpLists.Where(l => l.LookUpId == 5).ToList(),
                "Id",
                "Name"
            );

            // LookUpList tablosundan Öğrenim Durumu (LookUpId = 3) verilerini al
            ViewBag.OgrenimDurumlari = new SelectList(
                _context.LookUpLists.Where(l => l.LookUpId == 4).ToList(),
                "Id",
                "Name"
            );

            // LookUpList tablosundan Kan Grubu (LookUpId = 4) verilerini al
            ViewBag.KanGruplari = new SelectList(
                _context.LookUpLists.Where(l => l.LookUpId == 2).ToList(),
                "Id",
                "Name"
            );

            return View(new AddUserViewModel());
        }



        [HttpPost("AddUser")]
        [ValidateAntiForgeryToken]
        public IActionResult AddUser(AddUserViewModel model, IFormFile ProfileImage)
        {
            if (ModelState.IsValid)
            {
                // Hem TcKimlik hem de Email'i kontrol eden sorgu
                var existingUser = _context.Users.FirstOrDefault(u => u.Email == model.Email || u.TcKimlik == model.TcKimlik);

                if (existingUser != null)
                {
                    // Hangi alanın çakıştığını bul ve ona göre hata ekle
                    if (existingUser.Email == model.Email)
                    {
                        ModelState.AddModelError("Email", "Bu e-posta adresi zaten kullanılıyor.");
                    }
                    if (existingUser.TcKimlik == model.TcKimlik)
                    {
                        ModelState.AddModelError("TcKimlik", "Bu TC Kimlik numarası zaten kullanılıyor.");
                    }
                    return View(model);
                }

                var hashedPassword = HashHelper.HashPassword(model.Password);

                byte[] imageData = null;
                if (ProfileImage != null && ProfileImage.Length > 0)
                {
                    using (var ms = new MemoryStream())
                    {
                        ProfileImage.CopyTo(ms);
                        imageData = ms.ToArray();
                    }
                }

                var user = new User
                {
                    TcKimlik = model.TcKimlik,
                    BirthDate = model.BirthDate,
                    FullName = model.FullName ?? string.Empty,
                    Email = model.Email ?? string.Empty,
                    Password = hashedPassword,
                    About = model.About ?? string.Empty,
                    Address = model.Address ?? string.Empty,
                    Company = model.Company ?? string.Empty,
                    JobTitle = model.JobTitle ?? string.Empty,
                    Country = model.Country ?? string.Empty,
                    Phone = model.Phone,
                    Twitter = model.Twitter ?? string.Empty,
                    Facebook = model.Facebook ?? string.Empty,
                    Instagram = model.Instagram ?? string.Empty,
                    Puan = model.Puan ?? "0",
                    KayitTarihi = DateTime.Now, // Kayıt tarihi atanıyor

                    EhliyetId = model.EhliyetId,
                    CinsiyetId = model.CinsiyetId, // LookUpList'ten gelen Cinsiyet Id
                    MedeniDurumId = model.MedeniDurumId, // LookUpList'ten gelen Medeni Durum Id
                    OgrenimDurumuId = model.OgrenimDurumuId, // LookUpList'ten gelen Öğrenim Durumu Id
                    KanGrubuId = model.KanGrubuId, // LookUpList'ten gelen Kan Grubu Id
                    ProfileImage = imageData
                };

                _context.Users.Add(user);
                _context.SaveChanges();

                var role = _context.Roles.FirstOrDefault(r => r.Name == "User");
                if (role != null)
                {
                    var userRole = new UserRole
                    {
                        UserId = user.Id,
                        RoleId = role.Id
                    };
                    _context.UserRoles.Add(userRole);
                }

                var homeIndexPermission = _context.Permissions.FirstOrDefault(p => p.Controller == "Home" && p.Action == "Index");
                if (homeIndexPermission != null)
                {
                    var rolePermission = _context.RolePermissions
                        .SingleOrDefault(rp => rp.RoleId == role.Id && rp.PermissionId == homeIndexPermission.Id);

                    if (rolePermission == null)
                    {
                        rolePermission = new RolePermission
                        {
                            RoleId = role.Id,
                            PermissionId = homeIndexPermission.Id
                        };
                        _context.RolePermissions.Add(rolePermission);
                    }
                }

                _context.SaveChanges();

                TempData["SuccessMessage"] = "Kullanıcı başarıyla eklendi.";
                return RedirectToAction("ListUsers");
            }

            foreach (var state in ModelState)
            {
                foreach (var error in state.Value.Errors)
                {
                    System.Diagnostics.Debug.WriteLine($"Error in {state.Key}: {error.ErrorMessage}");
                }
            }

            return View(model);
        }

        [HttpGet("EditUser/{id}")]
        public IActionResult EditUser(int id)
        {
            var user = _context.Users.Find(id);
            if (user == null)
            {
                return NotFound();
            }

            // LookUpList'ten dropdown verilerini al
            ViewBag.Cinsiyetler = new SelectList(
                _context.LookUpLists.Where(l => l.LookUpId == 1).ToList(),
                "Id",
                "Name",
                user.CinsiyetId // Mevcut Cinsiyet seçimi
            );

            ViewBag.MedeniDurumlar = new SelectList(
                _context.LookUpLists.Where(l => l.LookUpId == 5).ToList(),
                "Id",
                "Name",
                user.MedeniDurumId // Mevcut Medeni Durum seçimi
            );

            ViewBag.OgrenimDurumlari = new SelectList(
                _context.LookUpLists.Where(l => l.LookUpId == 4).ToList(),
                "Id",
                "Name",
                user.OgrenimDurumuId // Mevcut Öğrenim Durumu seçimi
            );

            ViewBag.KanGruplari = new SelectList(
                _context.LookUpLists.Where(l => l.LookUpId == 2).ToList(),
                "Id",
                "Name",
                user.KanGrubuId // Mevcut Kan Grubu seçimi
            );
            ViewBag.Ehliyetler = new SelectList(
            _context.LookUpLists.Where(l => l.LookUpId == 3).ToList(),
            "Id",
            "Name",
            user.EhliyetId // Mevcut Kan Grubu seçimi
        );

            // Kullanıcı bilgilerini ViewModel'e aktar
            var model = new EditUserViewModel
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                About = user.About,
                Address = user.Address,
                Company = user.Company,
                JobTitle = user.JobTitle,
                Country = user.Country,
                Phone = user.Phone,
                Twitter = user.Twitter,
                Facebook = user.Facebook,
                Instagram = user.Instagram,
                Puan = user.Puan,
                ProfileImage = user.ProfileImage,
                TcKimlik = user.TcKimlik,
                BirthDate = user.BirthDate,
                CinsiyetId = user.CinsiyetId, // LookUpList'ten gelen Cinsiyet Id
                MedeniDurumId = user.MedeniDurumId, // LookUpList'ten gelen Medeni Durum Id
                OgrenimDurumuId = user.OgrenimDurumuId, // LookUpList'ten gelen Öğrenim Durumu Id
                KanGrubuId = user.KanGrubuId, // LookUpList'ten gelen Kan Grubu Id
                EhliyetId = user.EhliyetId
            };

            return View(model);
        }

        [HttpPost("EditUser/{id}")]
        public IActionResult EditUser(EditUserViewModel model, IFormFile? ProfileImage)
        {
            if (ModelState.IsValid)
            {
                var user = _context.Users.Find(model.Id);
                if (user == null)
                {
                    return NotFound();
                }

                // Kullanıcı bilgilerini güncelle
                user.FullName = model.FullName;
                user.Email = model.Email;
                user.About = model.About;
                user.Address = model.Address;
                user.Company = model.Company;
                user.JobTitle = model.JobTitle;
                user.Country = model.Country;
                user.Phone = model.Phone;
                user.Twitter = model.Twitter;
                user.Facebook = model.Facebook;
                user.Instagram = model.Instagram;
                user.Puan = model.Puan;
                user.TcKimlik = model.TcKimlik;
                user.BirthDate = model.BirthDate;
                user.CinsiyetId = model.CinsiyetId; // LookUpList'ten gelen Cinsiyet Id
                user.MedeniDurumId = model.MedeniDurumId; // LookUpList'ten gelen Medeni Durum Id
                user.KanGrubuId = model.KanGrubuId; // LookUpList'ten gelen Kan Grubu Id
                user.OgrenimDurumuId = model.OgrenimDurumuId; // LookUpList'ten gelen Öğrenim Durumu Id
                user.EhliyetId = model.EhliyetId; // LookUpList'ten gelen Öğrenim Durumu Id


                // Profil resmi güncelleniyorsa işle
                if (ProfileImage != null && ProfileImage.Length > 0)
                {
                    using (var ms = new MemoryStream())
                    {
                        ProfileImage.CopyTo(ms);
                        user.ProfileImage = ms.ToArray();
                    }
                }

                // Veritabanında güncelleme işlemi
                _context.Users.Update(user);
                _context.SaveChanges();

                TempData["SuccessMessage"] = "Kullanıcı başarıyla güncellendi.";
                return RedirectToAction("ListUsers");
            }

            // ModelState geçerli değilse dropdown'lar için verileri tekrar yükle
            ViewBag.Cinsiyetler = new SelectList(
                _context.LookUpLists.Where(l => l.LookUpId == 1).ToList(),
                "Id",
                "Name",
                model.CinsiyetId // Mevcut seçili değer
            );

            ViewBag.MedeniDurumlar = new SelectList(
                _context.LookUpLists.Where(l => l.LookUpId == 5).ToList(),
                "Id",
                "Name",
                model.MedeniDurumId // Mevcut seçili değer
            );

            ViewBag.OgrenimDurumlari = new SelectList(
                _context.LookUpLists.Where(l => l.LookUpId == 4).ToList(),
                "Id",
                "Name",
                model.OgrenimDurumuId // Mevcut seçili değer
            );

            ViewBag.Ehliyetler = new SelectList(
            _context.LookUpLists.Where(l => l.LookUpId == 3).ToList(),
            "Id",
            "Name",
            model.EhliyetId // Mevcut seçili değer
        );

            ViewBag.KanGruplari = new SelectList(
                _context.LookUpLists.Where(l => l.LookUpId == 2).ToList(),
                "Id",
                "Name",
                model.KanGrubuId // Mevcut seçili değer
            );

            return View(model);
        }



        [HttpGet("DeleteUser/{id}")]
        public IActionResult DeleteUser(int id)
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == id);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

        [HttpPost("ConfirmDeleteUser")]
        public IActionResult ConfirmDeleteUser(int id)
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == id);

            if (user != null)
            {
                _context.Users.Remove(user);
                _context.SaveChanges();
            }

            return RedirectToAction("ListUsers");
        }



    }

}

