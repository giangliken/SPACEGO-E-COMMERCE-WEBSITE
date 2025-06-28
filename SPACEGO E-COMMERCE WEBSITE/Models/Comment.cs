namespace SPACEGO_E_COMMERCE_WEBSITE.Models
{
    public class Comment
    {
        public int CommentId { get; set; }

        //Liên kết người dùng
        public string UserId { get; set; }
        public ApplicationUser? User { get; set; }

        //Liên kết sản phẩm
        public int ProductId { get; set; }
        public Product? Product { get; set; }

        //Nội dung bình luận
        public string Content { get; set; }
        public int Rating { get; set; }
        public bool isActive { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
