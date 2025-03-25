using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace _2280600691_NguyenDinhDo.Models
{
    public class Order
    {
        public int Id { get; set; }
        public string UserId { get; set; }

        [Required(ErrorMessage = "Shipping Address is required")]
        [Display(Name = "Shipping Address")]
        public string ShippingAddress { get; set; }

        public DateTime OrderDate { get; set; }
        public decimal TotalPrice { get; set; }

        public List<OrderDetail> OrderDetails { get; set; }

        public string Notes { get; set; }
    }
}
