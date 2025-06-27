using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;

namespace SPACEGO_E_COMMERCE_WEBSITE.Models.ViewModel
{
    public class WarehouseInputViewModel
    {
        public int? VariantId { get; set; }
        public int ProductId { get; set; }

        [BindNever]
        public string? ProductName { get; set; }

        [BindNever]
        public string VariantDisplay { get; set; } = "-";

        [BindNever]
        public int CurrentQuantity { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Số lượng nhập phải lớn hơn 0")]

        public int InputQuantity { get; set; }

        public bool IsVariant { get; set; }
    }
}
