using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;   // 標籤驗證所需的函式庫
using Todo.Models;
using Todo.ValidationAttributes;

namespace Todo.Dto
{
    public class TodoListPutDto
    {
        public Guid TodoId { get; set; }
        [TodoName]
        public string Name { get; set; }
        public bool Enable { get; set; }
        [Range(0,100)]
        public int Orders { get; set; }
        public ICollection<UploadFilePostDto> UploadFiles { get; set; }

    }
}
