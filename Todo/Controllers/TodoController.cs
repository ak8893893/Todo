using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using Todo.Models;
using System;
using Microsoft.EntityFrameworkCore;
using Todo.Dto;
using AutoMapper;
using Todo.Parameters;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using System.Xml.Linq;


// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Todo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TodoController : ControllerBase
    {
        private readonly TodoContext _todoContext;
        private readonly IMapper _iMapper;

        public TodoController(TodoContext todoContext, IMapper iMapper)
        {
            _todoContext = todoContext;
            _iMapper = iMapper;
        }


        //讀取所有資料的API
        // GET: api/<TodoController>
        [HttpGet]
        public IEnumerable<TodoListSelectDto> Get([FromQuery] TodoSelectParameters value)
        {
            var result = _todoContext.TodoLists.
                Include(a => a.UpdateEmployee).
                Include(a => a.InsertEmployee).
                Include(a => a.UploadFiles)
                .Select(a => a);

            // LINQ寫法
            //if(!string.IsNullOrEmpty(name) )
            //{
            //    result=result.Where(a=>a.Name.Contains(name));
            //}

            // C# 寫法
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

            return result.ToList().Select(a => ItemToDto(a));
        }

        // AutoMapper接口  
        // GET: api/<TodoController>/AutoMapper
        [HttpGet("AutoMapper")]
        public IEnumerable<TodoListSelectDto> GetAutoMapper([FromQuery] TodoSelectParameters value)
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

        // AutoMapper接口
        // GET: api/<TodoController>/AutoMapper/5
        [HttpGet("AutoMapper/{id}")]
        public TodoListSelectDto GetAutoMapperSingle(Guid id)
        {
            var result = (from a in _todoContext.TodoLists
                          where a.TodoId == id
                          select a)
                          .Include(a => a.UpdateEmployee)
                          .Include(a => a.InsertEmployee)
                          .SingleOrDefault();
            return _iMapper.Map<TodoListSelectDto>(result); ;
        }

        //讀取單筆資料的API
        // GET api/<TodoController>/5
        [HttpGet("{id}")]
        public TodoListSelectDto GetTodoList(Guid id)
        {
            var result = (from a in _todoContext.TodoLists
                          where a.TodoId == id
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

        // 標籤功能
        // GET api/<TodoController>/From/5
        [HttpGet("From/{id}")]
        public dynamic GetFrom([FromRoute] string id,
            [FromQuery] string id2,
            //[FromBody] string id3,
            [FromForm] string id4)
        {
            List<dynamic> result = new List<dynamic>();

            result.Add(id);
            result.Add(id2);
            //result.Add(id3);
            result.Add(id4);
            return result;
        }

        // 新增資料
        // POST api/<TodoController>
        [HttpPost]
        public ActionResult<TodoList> Post([FromBody] TodoList value)
        {
            _todoContext.TodoLists.Add(value);
            _todoContext.SaveChanges();

            return CreatedAtAction(nameof(GetTodoList), new { id = value.TodoId }, value);
        }

        //更新資料
        // PUT api/<TodoController>/5
        [HttpPut("{id}")]
        public IActionResult Put(Guid id, [FromBody] TodoList value)
        {
            if (id != value.TodoId)
            {
                return BadRequest("url id 與 body ID 不符合");
            }

            _todoContext.Entry(value).State = EntityState.Modified; // 修改新得到的值

            try
            {
                _todoContext.SaveChanges();         // 將更新後的新資料存入資料庫
            }
            catch (DbUpdateException)
            {
                if (!_todoContext.TodoLists.Any(e => e.TodoId == id))  // 如果傳入的id 找不到任何一樣 回傳沒找到該筆資料 
                {
                    return NotFound();
                }
                else
                {
                    return StatusCode(500, "存取發生錯誤");             // 如果都有也都合法 還是失敗 回傳伺服器端存取有問題 
                }

            }

            return NoContent();                     // 成功存取後回傳 204 NoContent
        }

        // 刪除資料
        // DELETE api/<TodoController>/5
        [HttpDelete("{id}")]
        public IActionResult Delete(Guid id)
        {
            var delete = _todoContext.TodoLists.Find(id);
            if (delete == null)
            {
                return NotFound("沒有找到你指定的資料");
            }
            _todoContext.TodoLists.Remove(delete);
            _todoContext.SaveChanges();

            return NoContent();
        }

        private static TodoListSelectDto ItemToDto(TodoList a)
        {
            List<UploadFileDto> updto = new List<UploadFileDto>();

            foreach(var temp in a.UploadFiles) 
            {
                UploadFileDto up = new UploadFileDto
                {
                    Name= temp.Name,
                    Src = temp.Src,
                    TodoId= temp.TodoId,
                    UploadFileId= temp.UploadFileId,
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
            };
        }

    }
}
