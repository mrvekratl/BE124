using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Data.Entities
{
    public class SellerRequestEntity
    {
        public int Id { get; set; } // Talep için benzersiz ID
        public int UserId { get; set; } // Kullanıcı ID'si (Bu, UserEntity ile ilişkilendirilir)
        public string RequestMessage { get; set; } // Kullanıcının talep mesajı
        public DateTime CreatedAt { get; set; } // Talebin oluşturulma tarihi
        public bool IsApproved { get; set; } // Admin tarafından onaylanıp onaylanmadığı

        
    }
}
