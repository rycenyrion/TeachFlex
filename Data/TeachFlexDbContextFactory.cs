using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using TeachFlex.Services;

namespace TeachFlex.Data
{
    public class TeachFlexDbContextFactory :
        IDesignTimeDbContextFactory<
            TeachFlexDbContext>
    {
        public TeachFlexDbContext CreateDbContext(
            string[] args)
        {
            IDatabasePathService
                databasePathService =
                    new DatabasePathService();

            databasePathService
                .EnsureDatabaseFolderExists();

            DbContextOptionsBuilder<
                TeachFlexDbContext>
                    optionsBuilder =
                        new DbContextOptionsBuilder<
                            TeachFlexDbContext>();

            optionsBuilder.UseSqlite(
                databasePathService
                    .ConnectionString);

            return new TeachFlexDbContext(
                optionsBuilder.Options);
        }
    }
}