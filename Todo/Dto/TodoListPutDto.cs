using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;   // 標籤驗證所需的函式庫
using Todo.Abstracts;
using Todo.Models;
using Todo.ValidationAttributes;

namespace Todo.Dto
{
    public class TodoListPutDto : TodoListEditDtoAbstract
    {
        public Guid TodoId { get; set; }
    }
}
