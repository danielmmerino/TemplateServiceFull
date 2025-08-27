using Microsoft.EntityFrameworkCore;
using TemplateService.Infrastructure.Data;
using TemplateService.Infrastructure.Models;

namespace TemplateService.Infrastructure.Repositories;

public class TodoRepository(AppDbContext db)
{
    public Task<List<TodoItem>> GetAsync() => db.Todos.AsNoTracking().ToListAsync();
    public async Task<TodoItem> AddAsync(TodoItem todo)
    {
        db.Todos.Add(todo);
        await db.SaveChangesAsync();
        return todo;
    }
}
