using StackTeste.Domain.Helpers.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StackTeste.Domain.Models
{
    public class Lead
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public LeadStatus Status { get; set; } = LeadStatus.New;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
    }
}
