using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;   // 標籤驗證所需的函式庫
using Todo.Models;
using Todo.ValidationAttributes;
using System.Linq;

namespace Todo.Dto
{
    //[StartEnd]
    //[Test(Tvalue = "good")]
    public class TodoListPostDto : IValidatableObject
    {
        //[RegularExpression("[a-z]")] // 字元要在 a-z 才能過
        //[EmailAddress(ErrorMessage ="名字請輸入電子信箱")]
        
        //[TodoName]
        // [TodoNameAttribute]  // 也可以寫成這樣 有繼承ValidationAttribute 的 class 後面如果有 Attribute 的話可以不用打
        public string Name { get; set; }
        public bool Enable { get; set; }
        [Range(0,100)]
        public int Orders { get; set; }
        public ICollection<UploadFilePostDto> UploadFiles { get; set; }

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        // 把這個DTO要做的驗證邏輯全部都寫在這邊
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // 名稱不能重複的驗證
            TodoContext _todoContext = (TodoContext)validationContext.GetService(typeof(TodoContext));

            var findName = from a in _todoContext.TodoLists
                           where a.Name == Name
                           select a;

            // 取得使用這個標籤的是哪個類別
            var dto = validationContext.ObjectInstance;

            if (dto.GetType() == typeof(TodoListPutDto))  // 如果類別是TodoListPutDto 執行下面的二次過濾
            {
                var dtoUpdate = (TodoListPutDto)dto;     // 第一行程式碼是將 validationContext.ObjectInstance 強制轉換為 TodoListPutDto 類型的變數，並且將其指定為 dtoUpdate 變數。這裡的 TodoListPutDto 是一個數據傳輸對象，用於更新現有待辦事項列表的名稱。
                findName = findName.Where(a => a.TodoId != dtoUpdate.TodoId);  // 第二行程式碼是使用 LINQ 查詢，從 _todoContext.TodoLists 中選擇所有名稱等於 name 的待辦事項，並且排除 TodoId 等於 dtoUpdate.TodoId 的待辦事項。這裡的 TodoId 是待辦事項的唯一標識符。這樣可以確保在更新現有待辦事項列表的名稱時，不會將待辦事項列表中的其他項目與之混淆。
            }


            if (findName.FirstOrDefault() != null)
            {
              yield  return new ValidationResult("已存在相同的待辦事項", new string[] { "Name" });
            }
            


            // 開始時間不能大於結束時間的驗證

            if (StartTime >= EndTime)
            {
                yield return new ValidationResult("開始時間不能大於結束時間", new string[] { "time" });
            }

        }
    }
}
