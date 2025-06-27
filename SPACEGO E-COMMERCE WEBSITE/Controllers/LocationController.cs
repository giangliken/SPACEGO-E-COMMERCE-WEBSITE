using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using System.Text;

namespace SPACEGO_E_COMMERCE_WEBSITE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LocationController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string token = "270723cc-4cd1-11f0-9b81-222185cb68c8";
        public LocationController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet("provinces")]
        public async Task<IActionResult> GetProvinces()
        {
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Add("Token", token);
            var response = await client.GetAsync("https://dev-online-gateway.ghn.vn/shiip/public-api/master-data/province");
            var json = await response.Content.ReadAsStringAsync();
            return Content(json, "application/json");
        }

        [HttpGet("districts/{provinceId}")]
        public async Task<IActionResult> GetDistricts(int provinceId)
        {
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Add("Token", token);
            var requestBody = new
            {
                province_id = provinceId
            };
            var requestContent = new StringContent(JsonConvert.SerializeObject(requestBody), System.Text.Encoding.UTF8, "application/json");

            var response = await client.PostAsync("https://dev-online-gateway.ghn.vn/shiip/public-api/master-data/district", requestContent);
            var json = await response.Content.ReadAsStringAsync();
            return Content(json, "application/json");
        }

        [HttpGet("wards/{districtId}")]
        public async Task<IActionResult> GetWards(int districtId)
        {
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Add("Token", token);
            var requestBody = new
            {
                district_id = districtId
            };
            var requestContent = new StringContent(JsonConvert.SerializeObject(requestBody), System.Text.Encoding.UTF8, "application/json");

            var response = await client.PostAsync("https://dev-online-gateway.ghn.vn/shiip/public-api/master-data/ward", requestContent);
            var json = await response.Content.ReadAsStringAsync();
            return Content(json, "application/json");
        }
        // ✅ TÍNH PHÍ VẬN CHUYỂN (mặc định)
        [HttpPost("shipping-fee")]
        public async Task<IActionResult> GetShippingFee([FromBody] JObject body)
        {
            try
            {


                // Ghi log body JSON gốc
                var jsonBody = body.ToString();
                Console.WriteLine("==> Received shipping body: " + jsonBody); // hoặc dùng logger

                // Nếu muốn parse từng trường để kiểm tra chi tiết
                var toDistrictId = (int?)body["to_district_id"];
                var toWardCode = (string)body["to_ward_code"];
                var items = body["items"]?.ToString();

                Console.WriteLine($"==> To District: {toDistrictId}, Ward: {toWardCode}");
                Console.WriteLine("==> Items: " + items);

                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Add("Token", "270723cc-4cd1-11f0-9b81-222185cb68c8");
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Add("ShopId", "192592");
                var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

                var response = await client.PostAsync("https://dev-online-gateway.ghn.vn/shiip/public-api/v2/shipping-order/fee", content);
                var json = await response.Content.ReadAsStringAsync();

                return Content(json, "application/json");
            }
            catch (Exception ex)
            {
                Console.WriteLine("==> Exception: " + ex.Message);
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("available-services")]
        public async Task<IActionResult> GetAvailableServices([FromBody] JObject body)
        {
            try
            {
                var fromDistrict = (int?)body["from_district"];
                var toDistrict = (int?)body["to_district"];
                var shopId = (int?)body["shop_id"];

                if (fromDistrict == null || toDistrict == null || shopId == null)
                    return BadRequest(new { error = "Thiếu tham số bắt buộc" });

                var requestBody = new
                {
                    from_district = fromDistrict,
                    to_district = toDistrict,
                    shop_id = shopId
                };

                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Add("Token", "270723cc-4cd1-11f0-9b81-222185cb68c8");
                client.DefaultRequestHeaders.Add("ShopId", "192592");
                var content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");

                var response = await client.PostAsync("https://dev-online-gateway.ghn.vn/shiip/public-api/v2/shipping-order/available-services", content);
                var json = await response.Content.ReadAsStringAsync();
                return Content(json, "application/json");
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

    }
}