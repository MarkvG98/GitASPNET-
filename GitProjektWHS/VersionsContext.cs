using Microsoft.EntityFrameworkCore;

namespace GitProjektWHS
{
    public class VersionsContext : DbContext 
    {
        public VersionsContext(DbContextOptions<VersionsContext> options) : base(options)
        {
                
        }
        public  DbSet<VersionsDatei> Datei { get; set; }
        public DbSet<VersionsObjekt> Versions { get; set; }
    }
}
