namespace SPACEGO_E_COMMERCE_WEBSITE.Models
{
    public class PostCategory
    {
        public int PostCategoryId { get; set; }
        public string CategoryName { get; set; }
        public ICollection<Post>? Posts { get; set; }
    }
}
