using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Todo.Dto;

#nullable disable

namespace Todo.Models
{
    public partial class TodoContext2 : TodoContext
    {
        public TodoContext2()
        {
        }

        public TodoContext2(DbContextOptions<TodoContext> options)
            : base(options)
        {
        }

        // 只要留下有新增的東西在2代就可以了
        public virtual DbSet<TodoListSelectDto> TodoListSelectDtos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // 這個動作就是把一代的所有方法都叫回來 才能呼叫他們
            modelBuilder.Entity<TodoListSelectDto>().HasNoKey();
        }

    }
}
