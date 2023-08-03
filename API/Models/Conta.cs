using System.ComponentModel.DataAnnotations.Schema;

namespace API.Models
{
    public class Conta
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; } 
    }
}
