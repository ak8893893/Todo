using System;
using System.Collections.Generic;
using Todo.Models;

namespace Todo.Dto
{
    public class TodoListSelectDto
    {
        public Guid TodoId { get; set; }
        public string Name { get; set; }
        public DateTime InsertTime { get; set; }
        public DateTime UpdateTime { get; set; }
        public bool Enable { get; set; }
        public int Orders { get; set; }
        public string InsertEmployeeQName { get; set; }
        public string UpdateEmployeeQName { get; set; }
        public ICollection<UploadFileDto> UploadFiles { get; set; }

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

    }
}
