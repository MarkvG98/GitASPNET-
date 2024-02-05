using Commons.Models;
using Microsoft.EntityFrameworkCore;

namespace GitProjektWHS.Models
{
    public class VersionsContext : DbContext 
    {
        public VersionsContext(DbContextOptions<VersionsContext> options) : base(options)
        {
                
        }
        public  DbSet<FileObject> Dateien { get; set; }
        public DbSet<VersionObject> Versionen { get; set; }
    }
}
