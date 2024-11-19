using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;


namespace Portal.Models.Admin
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string FullName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public string? About { get; set; }
        public string? Address { get; set; }
        public string? Company { get; set; }
        public string? JobTitle { get; set; }
        public string? Country { get; set; }
        [Required]
        [StringLength(11)]

        public string Phone { get; set; } = string.Empty;
        public string? Twitter { get; set; }
        public string? Facebook { get; set; }
        public string? Instagram { get; set; }
        public string? Puan { get; set; }
        public byte[]? ProfileImage { get; set; }
        [Required]
        [StringLength(11, MinimumLength = 11, ErrorMessage = "TC Kimlik numarası 11 haneli olmalıdır.")]
        public string TcKimlik { get; set; } = string.Empty;


        [Required]
        public DateTime BirthDate { get; set; }
        public DateTime? LastQuizDate { get; set; }
        // Foreign Key for KanGrubus

        // Foreign Key for Kan Grubu from LookUpList
        public int? KanGrubuId { get; set; }
        [ForeignKey("KanGrubuId")]
        public LookUpList? KanGrubu { get; set; } // Navigation property


        // New LookUpList Relationships
        public int? CinsiyetId { get; set; }
        [ForeignKey("CinsiyetId")]
        public LookUpList? Cinsiyet { get; set; }

        public int? OgrenimDurumuId { get; set; }
        [ForeignKey("OgrenimDurumuId")]
        public LookUpList? OgrenimDurumu { get; set; }

        public int? EhliyetId { get; set; }
        [ForeignKey("EhliyetId")]
        public LookUpList? Ehliyet { get; set; }

        public int? MedeniDurumId { get; set; }
        [ForeignKey("MedeniDurumId")]
        public LookUpList? MedeniDurum { get; set; }
        public int? SubeId { get; set; }
        public Sube? Sube { get; set; }

        public DateTime? KayitTarihi { get; set; } // Kullanıcının kayıt tarihi



        public ICollection<UserRole> UserRoles { get; set; }

    }
}
