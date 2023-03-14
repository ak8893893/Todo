using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;   // 標籤驗證所需的函式庫
using Todo.Models;
using Todo.ValidationAttributes;

namespace Todo.Dto
{
    [StartEnd]
    [Test(Tvalue = "good")]
    public class TodoListPostDto
    {
        //[RegularExpression("[a-z]")] // 字元要在 a-z 才能過
        //[EmailAddress(ErrorMessage ="名字請輸入電子信箱")]
        
        [TodoName]
        // [TodoNameAttribute]  // 也可以寫成這樣 有繼承ValidationAttribute 的 class 後面如果有 Attribute 的話可以不用打
        public string Name { get; set; }
        public bool Enable { get; set; }
        [Range(0,100)]
        public int Orders { get; set; }
        public ICollection<UploadFilePostDto> UploadFiles { get; set; }

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

    }
}
