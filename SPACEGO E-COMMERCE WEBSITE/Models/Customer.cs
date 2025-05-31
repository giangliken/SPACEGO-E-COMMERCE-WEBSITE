using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SPACEGO_E_COMMERCE_WEBSITE.Models
{
    public class Customer
    {
        public int Id { get; set; }

        [DisplayName("Username")]
        [Required(ErrorMessage ="Bắt buộc nhập Username")]
        [MaxLength(50,ErrorMessage ="Username không quá 50 ký tự")]
        public string UserName { get; set; }

        [DisplayName("Họ tên")]
        [Required(ErrorMessage ="Bắt buộc nhập họ tên")]
        [MaxLength(100, ErrorMessage = "Họ tên không quá 100 ký tự")]
        public string FullName { get; set; }

        [DisplayName("Email")]
        [Required(ErrorMessage ="Bắt buộc nhập Email")]
        [EmailAddress(ErrorMessage ="Email không đúng định dạng")]
        public string Email { get; set; }

        [DisplayName("Số điện thoại")]
        [Required(ErrorMessage ="Bắt buộc nhập SĐT")]
        [Phone(ErrorMessage ="Không đúng định dạng SĐT")]
        public string PhoneNumber { get; set; }

        [DisplayName("Mật khẩu")]
        [Required(ErrorMessage ="Bắt buộc nhập mật khẩu")]
        [DataType(DataType.Password)]
        public string PassWord { get; set; }

        [DisplayName("Địa chỉ")]
        [Required(ErrorMessage ="Bắt buộc nhập địa chỉ")]
        [MaxLength(200, ErrorMessage ="Địa chỉ không quá 200 ký tự")]
        public string Address { get; set; }
        public DateTime CreateDate { get; set; }
        public bool IsLocked { get; set; }

    }
}
