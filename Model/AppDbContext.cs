
using Eva2Auth.Model;
using Microsoft.EntityFrameworkCore;

public class AppDbContext:DbContext
    {
    public DbSet<User>TblUsers { get; set; }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=Eva2BackEnd;Integrated Security=True;");
    }
}

