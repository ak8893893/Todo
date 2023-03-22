using System.Collections.Generic;
using Todo.Dto;
using System.Linq;
using Todo.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Todo.Parameters;
using System;
using AutoMapper;

namespace Todo.Services
{
    public class TodoListService
    {
        private readonly TodoContext _todoContext;
        private readonly IMapper _iMapper;

        public TodoListService(TodoContext todoContext, IMapper mapper)
        {
            _todoContext = todoContext;
            _iMapper = mapper;
        }

        public List<TodoListSelectDto> 取得資料(TodoSelectParameters value)
        {
            
            var result = _todoContext.TodoLists.
                Include(a => a.UpdateEmployee).
                Include(a => a.InsertEmployee).
                Include(a => a.UploadFiles)
                .Select(a => a);
            
            if (!string.IsNullOrEmpty(value.name))
            {
                result = result.Where(a => a.Name.IndexOf(value.name) > -1);
            }

            if (value.enable != null)
            {
                result = result.Where(a => a.Enable == value.enable);
            }
            if (value.InsertTime != null)
            {
                result = result.Where(a => a.InsertTime.Date == value.InsertTime);
            }
            if (value.maxOrder != null & value.minOrder != null)
            {
                result = result.Where(a => a.Orders <= value.maxOrder && a.Orders >= value.minOrder);
            }
            //if (result == null || result.Count() <= 0)
            //{
            //    return NotFound("找不到資源");
            //}

            return result.ToList().Select(a => ItemToDto(a)).ToList();
        }

        public TodoListSelectDto 取得單筆資料(Guid TodoId)
        {
            var result = (from a in _todoContext.TodoLists
                          where a.TodoId == TodoId
                          select new TodoListSelectDto
                          {
                              Enable = a.Enable,
                              InsertEmployeeQName = a.InsertEmployee.Name + "(" + a.InsertEmployeeId + ")",
                              InsertTime = a.InsertTime,
                              Name = a.Name,
                              Orders = a.Orders,
                              TodoId = a.TodoId,
                              UpdateEmployeeQName = a.UpdateEmployee.Name + "(" + a.UpdateEmployeeId + ")",
                              UpdateTime = a.UpdateTime,
                              StartTime= a.StartTime,
                              EndTime= a.EndTime,
                              UploadFiles = (from b in _todoContext.UploadFiles
                                             where a.TodoId == b.TodoId
                                             select new UploadFileDto
                                             {
                                                 Name = b.Name,
                                                 Src = b.Src,
                                                 TodoId = b.TodoId,
                                                 UploadFileId = b.UploadFileId,
                                             }).ToList()
                          }).SingleOrDefault();
            return result;
        }

        public IEnumerable<TodoListSelectDto> 使用AutoMapper取得資料(TodoSelectParameters value)
        {
            var result = _todoContext.TodoLists.
               Include(a => a.UpdateEmployee).
               Include(a => a.InsertEmployee)
               .Include(a => a.UploadFiles)
               .Select(a => a);

            if (!string.IsNullOrEmpty(value.name))
            {
                result = result.Where(a => a.Name.IndexOf(value.name) > -1);
            }

            if (value.enable != null)
            {
                result = result.Where(a => a.Enable == value.enable);
            }

            if (value.InsertTime != null)
            {
                result = result.Where(a => a.InsertTime.Date == value.InsertTime);
            }

            if (value.maxOrder != null & value.minOrder != null)
            {
                result = result.Where(a => a.Orders <= value.maxOrder && a.Orders >= value.minOrder);
            }

            var map = _iMapper.Map<IEnumerable<TodoListSelectDto>>(result);

            return map;
        }

        public TodoList 新增資料(TodoList value)
        {
            // 將資料進行轉譯後再放入資料庫
            TodoList insert = new TodoList
            {

                // 先決定哪些資料是使用者可以填入的
                Name = value.Name,
                Enable = value.Enable,
                Orders = value.Orders,
                StartTime = value.StartTime,
                EndTime = value.EndTime,

                // 再來把系統決定的值放入
                InsertTime = DateTime.Now,
                UpdateTime = DateTime.Now,

                // 因為還沒有做使用者身分認證  所以身分的部分先寫死
                InsertEmployeeId = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                UpdateEmployeeId = Guid.Parse("59308743-99e0-4d5a-b611-b0a7facaf21e"),

                // 同時新增子資料
                UploadFiles = value.UploadFiles,
            };

            _todoContext.TodoLists.Add(insert);
            _todoContext.SaveChanges();

            return insert;
        }

        public TodoList 使用AutoMapper新增資料(TodoListPostDto value)
        {
            var map = _iMapper.Map<TodoList>(value);

            // 再來把系統決定的值放入
            map.InsertTime = DateTime.Now;
            map.UpdateTime = DateTime.Now;

            // 因為還沒有做使用者身分認證  所以身分的部分先寫死
            map.InsertEmployeeId = Guid.Parse("00000000-0000-0000-0000-000000000001");
            map.UpdateEmployeeId = Guid.Parse("59308743-99e0-4d5a-b611-b0a7facaf21e");

            // 把父子資料放入資料庫後存檔
            _todoContext.TodoLists.Add(map);
            _todoContext.SaveChanges();

            return map;
        }

        public int 更新資料(Guid id,  TodoList value)
        {
            _todoContext.Entry(value).State = EntityState.Modified; // 修改新得到的值 這個寫法是自動找到內容進行更新

            // _todoContext.TodoLists.Update(value); // 這個寫法也可以  可以直覺知道是更新什麼資料

            try
            {
                _todoContext.SaveChanges();         // 將更新後的新資料存入資料庫

                return _todoContext.SaveChanges();
            }
            catch (DbUpdateException)
            {
                if (!_todoContext.TodoLists.Any(e => e.TodoId == id))  // 如果傳入的id 找不到任何一樣 回傳沒找到該筆資料 
                {
                    return 404;
                }
                else
                {
                    return 500;             // 如果都有也都合法 還是失敗 回傳伺服器端存取有問題 
                }

            }
        }

        public int 刪除資料 (Guid id)
        {
            var delete = _todoContext.TodoLists.Find(id);
            if (delete != null)
            {
                _todoContext.TodoLists.Remove(delete);
            }
            return _todoContext.SaveChanges(); 
        }
        private static TodoListSelectDto ItemToDto(TodoList a)
        {
            List<UploadFileDto> updto = new List<UploadFileDto>();

            foreach (var temp in a.UploadFiles)
            {
                UploadFileDto up = new UploadFileDto
                {
                    Name = temp.Name,
                    Src = temp.Src,
                    TodoId = temp.TodoId,
                    UploadFileId = temp.UploadFileId,
                };
                updto.Add(up);


            };

            return new TodoListSelectDto
            {
                Enable = a.Enable,
                InsertEmployeeQName = a.InsertEmployee.Name + "(" + a.InsertEmployeeId + ")",
                InsertTime = a.InsertTime,
                Name = a.Name,
                Orders = a.Orders,
                TodoId = a.TodoId,
                UpdateEmployeeQName = a.UpdateEmployee.Name + "(" + a.UpdateEmployeeId + ")",
                UpdateTime = a.UpdateTime,
                UploadFiles = updto,
                StartTime = a.StartTime,
                EndTime = a.EndTime,
            };
        }
    }
}
