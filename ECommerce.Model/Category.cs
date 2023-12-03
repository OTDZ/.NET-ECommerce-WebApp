using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ECommerce.Model
{
    public class Category
    {
        // [Key] - Picked up automatically as primary key if named Id or CategoryId
        public int CategoryId { get; set; }
        [Required] // Not NULL
        [MaxLength(30)] // Validation
        public string Name { get; set; }
        [DisplayName("Display Order")] // Displayed with a space in the UI
        [Range(1, 100)] // Validation
        public int DisplayOrder { get; set; }
    }
}
