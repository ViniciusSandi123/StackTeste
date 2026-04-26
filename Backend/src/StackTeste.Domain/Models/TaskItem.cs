using StackTeste.Domain.Helpers.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TaskStatus = StackTeste.Domain.Helpers.Enums.TaskStatus;

namespace StackTeste.Domain.Models
{
    public class TaskItem
    {
        public int Id { get; set; }
        public int LeadId { get; set; }
        public string Title { get; set; } = null!;
        public DateTime? DueDate { get; set; }
        public TaskStatus Status { get; set; } = TaskStatus.Todo;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public Lead? Lead { get; set; }
    }
}
