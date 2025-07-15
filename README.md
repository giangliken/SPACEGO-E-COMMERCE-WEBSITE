# 🚀 SPACEGO E-COMMERCE WEBSITE

Website thương mại điện tử **SPACEGO** được phát triển bằng ASP.NET Core MVC.

## 📌 Mục lục

- [Giới thiệu](#giới-thiệu)
- [Công nghệ sử dụng](#công-nghệ-sử-dụng)
- [Tính năng nổi bật](#tính-năng-nổi-bật)
- [Tích hợp API](#tích-hợp-api)
- [Hướng dẫn cài đặt](#hướng-dẫn-cài-đặt)
- [Thông tin liên hệ](#thông-tin-liên-hệ)

---

## 📖 Giới thiệu

**SPACEGO** là một website thương mại điện tử cung cấp giải pháp bán hàng online, tích hợp vận chuyển và thanh toán trực tuyến. Lĩnh vực bán hàng: thiết bị điện tử

---

## 💻 Công nghệ sử dụng

### Ngôn ngữ & Framework:
- ASP.NET Core MVC

### Cơ sở dữ liệu:
- Microsoft SQL Server

---

## 🌟 Tính năng nổi bật

- Quản lý sản phẩm, đơn hàng, người dùng
- Hỗ trợ sản phẩm có biến thể (màu sắc, kích cỡ, v.v.)
- Tích hợp thanh toán và vận chuyển
- Giao diện responsive, thân thiện với người dùng
- Dashboard quản trị cho admin

---

## 🔗 Tích hợp API

- **GHN (Giao Hàng Nhanh)**: Tính toán phí vận chuyển theo địa chỉ người dùng.
- **VNPAY**: Thanh toán trực tuyến thông qua cổng VNPAY.
- **VIETQR**: Tạo mã QR để khách hàng chuyển khoản nhanh chóng.

---
## ⚙️ Hướng dẫn cài đặt

### Yêu cầu:
- .NET 9 hoặc mới hơn
- SQL Server 2022 hoặc mới hơn
- Visual Studio 2022 hoặc mới hơn

### Các bước setup:

1. Clone repo:
   ```bash
   git clone https://github.com/giangliken/SPACEGO-E-COMMERCE-WEBSITE.git
2. Sau khi clone về thành công thì mở project với Visual Studio 2022
3. Cập nhật chuỗi kết nối trong appsettings.json
   ```json
   "ConnectionStrings": 
   {
    "DefaultConnection": "Server="YourServerName";Database="YourDatabaseName";Trusted_Connection=True;TrustServerCertificate=True" 
   },
   //ServerName là tên Server trong Microsoft SQL

   //cấu hình dịch vụ đăng nhập với Goolge hoặc Facebook
   "Authentication": {
    "Google": {
      "ClientId": "YourClientId",
      "ClientSecret": "YourSecretId"
    },
    //ClientId và SecretId của Google lấy trên https://console.cloud.google.com/
    "Facebook": {
      "AppId": "YourAppId",
      "AppSecret": "YourAppSecret"
    }
    //Còn Facebook thì lên trang https://developers.facebook.com/
    },
   
   //cấu hình dịch vụ gửi mail
   "Email": {
    "Username": "YourEmail",
    "Password": "YourPassWord"
    },
    //cấu hình dịch vụ GHN
   "GHN": {
    "Token": "YourGHNToken",
    "ShopId": YourShopId
    },

    //Cấu hình dịch vụ VNPAY
    "Vnpay": {
    "BaseUrl": "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html",
    "ReturnUrl": "https://localhost:5001/VnPay/PaymentReturn",
    "TmnCode": "YourTmnCode",
    "HashSecret": "YourHashSecret",
    "Version": "2.1.0",
    "Command": "pay",
    "CurrCode": "VND",
    "Locale": "vn"
    },
    "PaymentCallBack": {
    "ReturnUrl": "https://localhost:44386/Home/PaymentReturn"
     },
    "TimeZoneId": "SE Asia Standard Time",

4. Mở Console trong Nuget Package chạy lệnh
- Add-Migration Khoi_tao_du_an
5. Sau đó chạy tiếp lệnh 
- Update-Database
6. Cuối cùng khởi chạy dự án. TOWKTEAM chúc bạn thành công!




