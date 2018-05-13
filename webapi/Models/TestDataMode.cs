
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace webapi.Models{
  public class TestData{

    [Key]
    public int id {get; set;} 
    public int numeric {get ; set;} 
    
    [Required]
    [StringLength(5)]
    public string nominal {get; set;}
  };

  public class TestDataContext : DbContext
  {

    public TestDataContext(DbContextOptions<TestDataContext> options)
        : base(options)
    {
    }
    public DbSet<TestData> testElements { get; set; }
  }
}