using System.ComponentModel.DataAnnotations;

namespace SPACEGO_E_COMMERCE_WEBSITE.Models
{
    public class Order
    {
        public int OrderId { get; set; }
        public decimal? Total { get; set; }
        public decimal? ShippingFee { get; set; }
        public DateTime? OrderDate { get; set; }
        [Required(ErrorMessage = "Vui lòng chọn phương thức thanh toán")]
        public string PaymentMethod { get; set; }
        public string OrderStatus { get; set; }

        public List<OrderProduct> OrderProducts { get; set; }

        // Thông tin người nhận hàng
        [Required(ErrorMessage = "Vui lòng nhập họ và tên")]
        public string FullName { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        public string PhoneNumber { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập email")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        public string Email { get; set; }

        // Thông tin địa chỉ
        [Required(ErrorMessage = "Vui lòng chọn tỉnh/thành phố")]
        public int ProvinceId { get; set; }
        public Province? Province { get; set; }
        [Required(ErrorMessage = "Vui lòng chọn quận/huyện")]

        public int DistrictID { get; set; }
        public District? District { get; set; }
        [Required(ErrorMessage = "Vui lòng chọn xã/phường")]

        // Nếu bạn có bảng Ward thì thêm:
        public string WardID { get; set; }
        public Ward? Ward { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ chi tiết")]
        public string AddressDetail { get; set; } // Số nhà, đường, phường...

        // Người đặt hàng
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
    }
}
