using System;
using System.Collections.Generic;
using Todo.Models;

namespace Todo.Dto
{
    public class TodoListPostDto
    {
        public string Name { get; set; }
        public bool Enable { get; set; }
        public int Orders { get; set; }
        public ICollection<UploadFilePostDto> UploadFiles { get; set; }

    }
}
