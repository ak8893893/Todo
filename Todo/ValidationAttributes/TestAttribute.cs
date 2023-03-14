using System.ComponentModel.DataAnnotations;
using Todo.Models;
using System.Linq;
using Todo.Dto;

namespace Todo.ValidationAttributes
{
    public class TestAttribute : ValidationAttribute
    {
        private string _tvalue; // 先宣告一個私有變數
        public string Tvalue = "de2";

        public TestAttribute(string tvalue = "de")    // 用建構子把值傳入這個模型 = "xx" 表示預設值為xx
        {
            _tvalue = tvalue;                  // 把值放入 私有變數 _tvalue
        }

        protected override ValidationResult IsValid (object value, ValidationContext validationContext)
        {
           

            var st = (TodoListPostDto)value;

            
            return new ValidationResult(Tvalue, new string[] {"tvalue"});
        }
    }
}
