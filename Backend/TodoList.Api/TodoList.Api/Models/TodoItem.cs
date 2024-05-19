using System;
using System.ComponentModel.DataAnnotations;

namespace TodoList.Api.Models
{
    public class TodoItem
    {
        [Required(ErrorMessage = "ID is required")]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; }

        public bool IsCompleted { get; set; }
    }
}
