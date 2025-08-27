using Microsoft.EntityFrameworkCore;
using TemplateService.Infrastructure.Models;

namespace TemplateService.Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<TodoItem> Todos => Set<TodoItem>();
}
