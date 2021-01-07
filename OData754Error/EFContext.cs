using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace OData_754_Error
{
    public class EFContext : DbContext
    {
        public EFContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ODataEntity>(x =>
            {
                x.HasKey(y => y.Id);
            });
        }
    }
}
