using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;   // 標籤驗證所需的函式庫
using Todo.Models;
using Todo.ValidationAttributes;
using System.Linq;
using Todo.Abstracts;

namespace Todo.Dto
{
    //[StartEnd]
    //[Test(Tvalue = "good")]
    public class TodoListPostDto : TodoListEditDtoAbstract
    {
        
    }
}
