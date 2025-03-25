using System.ComponentModel.DataAnnotations;

namespace _2280600691_NguyenDinhDo.Models
{
    public class CartItem
    {
        [Key] // Add this attribute to define the primary key
        public int Id { get; set; } // Primary Key

        public int ProductId { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
    }

}
