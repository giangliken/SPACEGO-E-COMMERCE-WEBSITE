namespace SPACEGO_E_COMMERCE_WEBSITE.Models
{
    public class Ward
    {   
        public string WardID { get; set; }
        public string WardName { get; set; }

        public int DistrictID { get; set; }
        public District District { get; set; }
    }
}
