using System.ComponentModel.DataAnnotations;
using Todo.Models;
using System.Linq;
using Todo.Dto;

namespace Todo.ValidationAttributes
{
    public class TodoNameAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid (object value, ValidationContext validationContext)
        {
            TodoContext _todoContext = (TodoContext)validationContext.GetService(typeof(TodoContext));

            var name = (string)value;

            var findName = from a in _todoContext.TodoLists
                           where a.Name == name 
                           select a;

            // 取得使用這個標籤的是哪個類別
            var dto = validationContext.ObjectInstance;

            if (dto.GetType() == typeof(TodoListPutDto))  // 如果類別是TodoListPutDto 執行下面的二次過濾
            {
                var dtoUpdate = (TodoListPutDto) dto;     // 第一行程式碼是將 validationContext.ObjectInstance 強制轉換為 TodoListPutDto 類型的變數，並且將其指定為 dtoUpdate 變數。這裡的 TodoListPutDto 是一個數據傳輸對象，用於更新現有待辦事項列表的名稱。
                findName = findName.Where(a => a.TodoId != dtoUpdate.TodoId);  // 第二行程式碼是使用 LINQ 查詢，從 _todoContext.TodoLists 中選擇所有名稱等於 name 的待辦事項，並且排除 TodoId 等於 dtoUpdate.TodoId 的待辦事項。這裡的 TodoId 是待辦事項的唯一標識符。這樣可以確保在更新現有待辦事項列表的名稱時，不會將待辦事項列表中的其他項目與之混淆。
            }


            if (findName.FirstOrDefault() != null) 
            {
                return new ValidationResult("已存在相同的待辦事項");
            }

            return ValidationResult.Success;
        }
    }
}
