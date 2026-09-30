using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TeachFlex.Data;
using TeachFlex.Repositories;

namespace TeachFlex.Services
{
    public interface IDatabaseInitializationService
    {
        Task InitializeAsync(
            CancellationToken cancellationToken =
                default);
    }

    public class DatabaseInitializationService :
        IDatabaseInitializationService
    {
        private readonly IDbContextFactory<
            TeachFlexDbContext>
                _dbContextFactory;

        private readonly
            IDefaultSubjectCatalogService
                _defaultSubjectCatalogService;
        private readonly
    ISeniorHighSubjectCatalogService
        _seniorHighSubjectCatalogService;

        private readonly
            IKindergartenCompetencyCatalogService
                _kindergartenCompetencyCatalogService;

        private readonly
            IKindergartenRecordRepository
                _kindergartenRecordRepository;

        public DatabaseInitializationService(
            IDbContextFactory<TeachFlexDbContext>
                dbContextFactory,
           IDefaultSubjectCatalogService
    defaultSubjectCatalogService,
ISeniorHighSubjectCatalogService
    seniorHighSubjectCatalogService,
IKindergartenCompetencyCatalogService
                kindergartenCompetencyCatalogService,
            IKindergartenRecordRepository
                kindergartenRecordRepository)
        {
            _dbContextFactory =
                dbContextFactory;

            _defaultSubjectCatalogService =
                defaultSubjectCatalogService;
            _seniorHighSubjectCatalogService =
    seniorHighSubjectCatalogService;

            _kindergartenCompetencyCatalogService =
                kindergartenCompetencyCatalogService;

            _kindergartenRecordRepository =
                kindergartenRecordRepository;
        }

        public async Task InitializeAsync(
            CancellationToken cancellationToken =
                default)
        {
            await using TeachFlexDbContext database =
                await _dbContextFactory
                    .CreateDbContextAsync(
                        cancellationToken);

            await database.Database
                .MigrateAsync(
                    cancellationToken);

            await _defaultSubjectCatalogService
                .SeedAsync(
                    cancellationToken);
            await _seniorHighSubjectCatalogService
    .SeedAsync(
        cancellationToken);

            await _kindergartenRecordRepository
                .SaveCompetenciesAsync(
                    _kindergartenCompetencyCatalogService
                        .CreateOfficialCompetencies(),
                    cancellationToken);
        }
    }
}