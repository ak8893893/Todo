using System.ComponentModel.DataAnnotations;
using Todo.Models;
using System.Linq;
using Todo.Dto;

namespace Todo.ValidationAttributes
{
    public class StartEndAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid (object value, ValidationContext validationContext)
        {
           

            var st = (TodoListPostDto)value;

            if (st.StartTime >= st.EndTime)
            {
                return new ValidationResult("開始時間不能大於結束時間", new string[] { "time" } );
            }
            
            return ValidationResult.Success;
        }
    }
}
