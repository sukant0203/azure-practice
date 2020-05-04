using Microsoft.EntityFrameworkCore;

namespace create_azure_vm.Models
{
    public class VmResourceContext : DbContext
    {
        public VmResourceContext(DbContextOptions<VmResourceContext> options)
            : base(options)
        {
        }

        public DbSet<VmResources> VmResources { get; set; }
    }
}