using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Portal.ViewModels.Admin
{

    public class EditUserViewModel
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Full Name")]
        public string FullName { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        public string? About { get; set; }
        public string? Address { get; set; }
        public string? Company { get; set; }
        public string? JobTitle { get; set; }
        public string? Country { get; set; }
        [Required]

        public string Phone { get; set; }
        public string? Twitter { get; set; }
        public string? Facebook { get; set; }
        public string? Instagram { get; set; }
        public string? Puan { get; set; }
        public byte[]? ProfileImage { get; set; }
        [Required]
        public string TcKimlik { get; set; }
        [Required]

        public int? CinsiyetId { get; set; } // LookUpList'ten gelen Cinsiyet Id
        public int? MedeniDurumId { get; set; } // LookUpList'ten gelen Medeni Durum Id
        public int? OgrenimDurumuId { get; set; } // LookUpList'ten gelen Öğrenim Durumu Id
        public int? EhliyetId { get; set; } // LookUpList'ten gelen Kan Grubu Id

        public int? KanGrubuId { get; set; } // LookUpList'ten gelen Kan Grubu Id

        public DateTime BirthDate { get; set; }

        // Base64 string for displaying the image
        public string? ProfileImageBase64 => ProfileImage != null && ProfileImage.Length > 0
            ? $"data:image/png;base64,{Convert.ToBase64String(ProfileImage)}"
            : string.Empty;

    }

}

