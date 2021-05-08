using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace RabbitMQWeb.Watermark.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [StringLength(100)]
        public string Name { get; set; }

        [Column(TypeName ="decimal(18,2)")]
        public decimal Price { get; set; }

        [Range(1,100)]
        public int Stok { get; set; }

        [StringLength(200)]
        public string Resimİsmi { get; set; }//Guid ile oluşturduğum için resimler unique olacak. Resim yolu dersek ilerde klasör değişikliğinde sorun olur
    }
}
