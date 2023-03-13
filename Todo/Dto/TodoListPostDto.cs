using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;   // 標籤驗證所需的函式庫
using Todo.Models;

namespace Todo.Dto
{
    public class TodoListPostDto
    {
        //[RegularExpression("[a-z]")] // 字元要在 a-z 才能過
        //[EmailAddress(ErrorMessage ="名字請輸入電子信箱")]
        public string Name { get; set; }
        public bool Enable { get; set; }
        [Range(0,100)]
        public int Orders { get; set; }
        public ICollection<UploadFilePostDto> UploadFiles { get; set; }

    }
}
