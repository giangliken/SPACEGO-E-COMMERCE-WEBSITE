namespace SPACEGO_E_COMMERCE_WEBSITE.Models
{
    public class Post
    {
        public int PostId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int UserId { get; set; }
        public ApplicationUser? User { get; set; }
        public int PostStatusId { get; set; }
        public ICollection<Comment>? Comments { get; set; }
        public ICollection<PostCategory>? PostCategories { get; set; }
    }

    public class PostCategory
    {
        public int PostCategoryId { get; set; }
        public string CategoryName { get; set; }
        public ICollection<Post>? Posts { get; set; }
    }
}
