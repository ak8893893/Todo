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
