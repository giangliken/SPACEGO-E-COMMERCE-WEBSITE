using System;
using System.ComponentModel.DataAnnotations;

namespace SPACEGO_E_COMMERCE_WEBSITE.Models
{
    public class Post
    {
        public int PostId { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Content { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }

        public string? UserId { get; set; }
        public ApplicationUser? User { get; set; }

        public bool IsPublished { get; set; } = true;

        public int? PostCategoryId { get; set; }
        public PostCategory? PostCategory { get; set; }
    }
}
