using System;
using Todo.Models;

namespace Todo.Dto
{
    public class UploadFilePostDto
    {
        public string Name { get; set; }
        public string Src { get; set; }
        
        // 有設外鍵父子新增的話這個key也不會要
        // public Guid TodoId { get; set; }

    }
}
