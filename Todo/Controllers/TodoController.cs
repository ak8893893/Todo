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
        public IActionResult Get([FromQuery] TodoSelectParameters value)
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

            if (result == null || result.Count() <= 0) 
            {
                return NotFound("找不到資源");
            }

            return Ok(result.ToList().Select(a => ItemToDto(a)));
        }

        //讀取單筆資料的API
        // GET api/<TodoController>/5
        // 改成用 IActionResult
        [HttpGet("{id}")]
        public IActionResult GetTodoList(Guid id)
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

            if (result == null )
            {
                return NotFound("找不到ID: "+id.ToString()+" 的資料");
            }

            return Ok( result);
        }

        //讀取所有資料的API   使用SQL指令
        // GET: api/<TodoController>/GetSQL
        [HttpGet("GetSQL")]
        public IEnumerable<TodoList> GetSQL(string name)
        {
            string sql = "select * from todolist where 1=1";   // 先做一個基礎SQL語句 搜尋所有資料

            if (!string.IsNullOrEmpty(name))                   // 如果query有輸入name 的值的話
            {
                sql = sql + "and name like N'%" + name + "%'"; // 包含name裡面的值的name 會被搜到
            }

            var result = _todoContext.TodoLists.FromSqlRaw(sql);

            return result;
        }

        //讀取所有資料的API   使用SQL指令
        // GET: api/<TodoController>/GetSQLDto
        [HttpGet("GetSQLDto")]
        public IEnumerable<TodoListSelectDto> GetSQLDto(string name)
        {
            string sql = @"select [TodoId]
             ,a.[Name]
             ,[InsertTime] 
             ,[UpdateTime] 
             ,[Enable]
             ,[Orders]
             ,b.Name as InsertEmployeeQName
             ,c.Name as UpdateEmployeeQName
                From [Todolist] a
                join Employee b on a.InsertEmployeeId=b.EmployeeId
                join Employee c on a.UpdateEmployeeId=c.EmployeeId where 1=1";

            if (!string.IsNullOrWhiteSpace(name))                   // 如果query有輸入name 的值的話
            {
                sql = sql + "and name like N'%" + name + "%'"; // 包含name裡面的值的name 會被搜到
            }

            var result = _todoContext.ExecSQL<TodoListSelectDto>(sql);

            return result;
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

            // 將資料進行轉譯後再放入資料庫
            TodoList insert = new TodoList
            {
                // 先決定哪些資料是使用者可以填入的
                Name= value.Name,
                Enable= value.Enable,
                Orders= value.Orders,

                // 再來把系統決定的值放入
                InsertTime = DateTime.Now,
                UpdateTime = DateTime.Now,
                
                // 因為還沒有做使用者身分認證  所以身分的部分先寫死
                InsertEmployeeId = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                UpdateEmployeeId = Guid.Parse("59308743-99e0-4d5a-b611-b0a7facaf21e"),
            };

            _todoContext.TodoLists.Add(insert);
            _todoContext.SaveChanges();

            return CreatedAtAction(nameof(GetTodoList), new { id = insert.TodoId }, insert);
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
