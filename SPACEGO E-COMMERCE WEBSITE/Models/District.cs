namespace SPACEGO_E_COMMERCE_WEBSITE.Models
{
    public class District
    {
        public int DistrictID { get; set; }
        public string DistrictName { get; set; }

        public int ProvinceID { get; set; }
        public Province Province { get; set; }

    }
}
