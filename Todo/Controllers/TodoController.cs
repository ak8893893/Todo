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
using Microsoft.Data.SqlClient;


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

            if (result == null)
            {
                return NotFound("找不到ID: " + id.ToString() + " 的資料");
            }

            return Ok(result);
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
                Name = value.Name,
                Enable = value.Enable,
                Orders = value.Orders,

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

            return CreatedAtAction(nameof(GetTodoList), new { id = insert.TodoId }, insert);
        }

        // 新增子資料
        // POST api/<TodoController>/UploadFile/{TodoId}
        [HttpPost("UploadFile/{TodoId}")]
        public ActionResult<UploadFile> PostChild(Guid TodoId, [FromBody] UploadFilePostDto value)
        {
            // 先檢查有沒有這筆父資料
            if (!_todoContext.TodoLists.Any(a => a.TodoId == TodoId))
            {
                return NotFound("沒有這筆資料 ID: " + TodoId.ToString());
            }

            // 將資料進行轉譯後再放入資料庫
            UploadFile insert = new UploadFile
            {
                // 先決定哪些資料是使用者可以填入的
                Name = value.Name,
                Src = value.Src,    // 上傳檔案路徑這個之後教
                TodoId = TodoId
            };

            _todoContext.UploadFiles.Add(insert);
            _todoContext.SaveChanges();

            return CreatedAtAction(nameof(GetTodoList), new { id = insert.TodoId }, insert);
        }


        // 新增子資料 AutoMapper
        // POST api/<TodoController>/UploadFile/{TodoId}
        [HttpPost("UploadFileAutoMapper/{TodoId}")]
        public ActionResult<UploadFile> PostChildAutoMapper(Guid TodoId, [FromBody] UploadFilePostDto value)
        {
            // 先檢查有沒有這筆父資料
            if (!_todoContext.TodoLists.Any(a => a.TodoId == TodoId))
            {
                return NotFound("沒有這筆資料 ID: " + TodoId.ToString());
            }

            var map = _iMapper.Map<UploadFile>(value);

            map.TodoId = TodoId; // 要給這筆資料TodoId才能放進去

            _todoContext.UploadFiles.Add(map);
            _todoContext.SaveChanges();

            return CreatedAtAction(nameof(GetTodoList), new { id = map.TodoId }, map);
        }



        // 同時新增父子資料 在沒有FK(外鍵的情況)
        // POST api/<TodoController>/Nofk
        [HttpPost("Nofk")]
        public ActionResult<TodoList> PostNoFK([FromBody] TodoListPostDto value)
        {
            // 分成兩個階段 先做好爸爸 存檔後取得爸爸的todoID再新增兒子進去爸爸下面

            // 爸爸部分
            
            // 將資料進行轉譯後再放入資料庫
            TodoList insert = new TodoList
            {
                // 先決定哪些資料是使用者可以填入的
                Name = value.Name,
                Enable = value.Enable,
                Orders = value.Orders,

                // 再來把系統決定的值放入
                InsertTime = DateTime.Now,
                UpdateTime = DateTime.Now,

                // 因為還沒有做使用者身分認證  所以身分的部分先寫死
                InsertEmployeeId = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                UpdateEmployeeId = Guid.Parse("59308743-99e0-4d5a-b611-b0a7facaf21e"),

                // 同時新增子資料 但因為我們這邊要當作沒有外鍵 所以先註解(沒外鍵的狀況下做了也不會有事情發生)
                //UploadFiles = value.UploadFiles,
            };

            // 把爸爸的資料放入資料庫後存檔
            _todoContext.TodoLists.Add(insert);
            _todoContext.SaveChanges();


            // 兒子部分
            foreach (var temp in value.UploadFiles)
            {
                UploadFile insert2 = new UploadFile
                {
                    Name = temp.Name,
                    Src = temp.Src,
                    TodoId = insert.TodoId,
                };
                _todoContext.UploadFiles.Add(insert2);  // 加入資料庫
            }

            // 存檔
            _todoContext.SaveChanges();

            return CreatedAtAction(nameof(GetTodoList), new { id = insert.TodoId }, insert);
        }

        // 同時新增父子資料用PostDto的情況
        // POST api/<TodoController>/Dto
        [HttpPost("PostDto")]
        public ActionResult<TodoList> PostDto([FromBody] TodoListPostDto value)
        {
            List<UploadFile> upl = new List<UploadFile>();

            foreach (var temp in value.UploadFiles)
            {
                UploadFile up = new UploadFile
                {
                    Name = temp.Name,
                    Src = temp.Src,
                };
                upl.Add(up);
            }

            // 將資料進行轉譯後再放入資料庫
            TodoList insert = new TodoList
            {
                // 先決定哪些資料是使用者可以填入的
                Name = value.Name,
                Enable = value.Enable,
                Orders = value.Orders,

                // 再來把系統決定的值放入
                InsertTime = DateTime.Now,
                UpdateTime = DateTime.Now,

                // 因為還沒有做使用者身分認證  所以身分的部分先寫死
                InsertEmployeeId = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                UpdateEmployeeId = Guid.Parse("59308743-99e0-4d5a-b611-b0a7facaf21e"),

                // 同時新增子資料
                UploadFiles = upl
            };

            // 把父子資料放入資料庫後存檔
            _todoContext.TodoLists.Add(insert);
            _todoContext.SaveChanges();

            return CreatedAtAction(nameof(GetTodoList), new { id = insert.TodoId }, insert);
        }

        // Post AutoMapper
        // POST api/<TodoController>/AutoMapperPost
        [HttpPost("AutoMapperPost")]
        public ActionResult<TodoList> PostAutoMapper([FromBody] TodoListPostDto value)
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

            return CreatedAtAction(nameof(GetTodoList), new { id = map.TodoId }, map);
        }

        // 使用內建的函式進行對應來新增資料 .CurrentValues.SetValues()
        // POST api/<TodoController>/DefaultMapperPost
        [HttpPost("DefaultMapperPost")]
        public ActionResult<TodoList> DefaultMapperPost([FromBody] TodoListPostDto value)
        {

            TodoList insert = new TodoList {

            // 再來把系統決定的值放入
            InsertTime = DateTime.Now,
            UpdateTime = DateTime.Now,

            // 因為還沒有做使用者身分認證  所以身分的部分先寫死
            InsertEmployeeId = Guid.Parse("00000000-0000-0000-0000-000000000001"),
            UpdateEmployeeId = Guid.Parse("59308743-99e0-4d5a-b611-b0a7facaf21e"),

        };


            // 把父資料放入資料庫後存檔
            _todoContext.TodoLists.Add(insert).CurrentValues.SetValues(value);  // 用這個內建的函式只能放值到有對應名稱的欄位 沒對應到的欄位(例如子資料 就要存檔後再另外添加)
            _todoContext.SaveChanges();

            // 把沒有對應到名稱的資料用迴圈放入 由於子資料裡面的名稱都對應的到所以不用手動田填入
            foreach(var temp in value.UploadFiles)
            {
                _todoContext.UploadFiles.Add(new UploadFile
                {
                    TodoId = insert.TodoId        // TodoID是我們剛剛新增的那一筆資料  所以要在存檔後取得他的todoID拿來放入這筆子資料的FK
                }).CurrentValues.SetValues(temp); // 把剩餘的資料用內建函式進行對應放入
                
            }

            _todoContext.SaveChanges();           // 所有子資料都放入後進行存檔


            return CreatedAtAction(nameof(GetTodoList), new { id = insert.TodoId }, insert);
        }

        // 使用SQL來新增資料 .CurrentValues.SetValues()  這邊只示範父親的部分就好  子資料就一樣的方式去下SQL指令
        // POST api/<TodoController>/DefaultMapperPost
        [HttpPost("PostSQL")]
        public ActionResult<TodoList> PostSQL([FromBody] TodoListPostDto value)
        {

            // 這邊如果有一些攻擊性的語法一樣是幫你轉換成單純字串
            var name = new SqlParameter("name", value.Name);

            // SQL insert 資料如下
            string sql = @"INSERT INTO [dbo].[TodoList]
            ([Name]
            ,[InsertTime]
            ,[UpdateTime]
            ,[Enable]
            ,[Orders]
            ,[InsertEmployeeId]
            ,[UpdateEmployeeId])
            VALUES
            (@name,'"+DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")+"','"+DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")+"','"+value.Enable+"',"+value.Orders+ ",'00000000-0000-0000-0000-000000000001','59308743-99e0-4d5a-b611-b0a7facaf21e')";
            // 加個N 避免編碼出現問題中文會變成????
            // @name 就是跟上面那個sqlParameter一樣名字的那個資料


            // 發送SQL指令後就執行了   所以不用另外存  
            // 如果後面有很多可以放入很多
            var result = _todoContext.Database.ExecuteSqlRaw(sql,name );
            

            


            return Ok(result);
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
