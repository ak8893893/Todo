using AutoMapper;
using System;
using Todo.Dto;
using Todo.Models;

namespace Todo.Profiles
{
    public class TodoListProfile : Profile
    {
        public TodoListProfile()
        {
            CreateMap<TodoList, TodoListSelectDto>()
                .ForMember(
                a => a.InsertEmployeeQName, // 前項為目的地欄位
                b => b.MapFrom(c => c.InsertEmployee.Name + "(" + c.InsertEmployeeId + ")")) // 後項為這個欄位資料的來源
                .ForMember(
                a => a.UpdateEmployeeQName,
                b => b.MapFrom(c => c.UpdateEmployee.Name + "(" + c.UpdateEmployeeId + ")"))
                ;
            CreateMap<TodoListPostDto, TodoList>();

            CreateMap<TodoListPutDto, TodoList>().ForMember(
                a => a.UpdateTime, // 前項為目的地欄位
                b => b.MapFrom(c => DateTime.Now)) // 後項為這個欄位資料的來源
                .ForMember(
                a => a.UpdateEmployeeId,
                b => b.MapFrom(c => Guid.Parse("59308743-99e0-4d5a-b611-b0a7facaf21e")))
                ;
        }
    }
}
