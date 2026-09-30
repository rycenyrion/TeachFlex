using System;
using System.IO;

namespace TeachFlex.Services
{
    public interface IDatabasePathService
    {
        string DatabaseFolder
        {
            get;
        }

        string DatabasePath
        {
            get;
        }

        string ConnectionString
        {
            get;
        }

        void EnsureDatabaseFolderExists();
    }

    public class DatabasePathService :
        IDatabasePathService
    {
        public DatabasePathService()
        {
            DatabaseFolder =
                Path.Combine(
                    Environment.GetFolderPath(
                        Environment.SpecialFolder
                            .LocalApplicationData),
                    "TeachFlex",
                    "Data");

            DatabasePath =
                Path.Combine(
                    DatabaseFolder,
                    "teachflex.db");
        }

        public string DatabaseFolder
        {
            get;
        }

        public string DatabasePath
        {
            get;
        }

        public string ConnectionString =>
            $"Data Source={DatabasePath}";

        public void EnsureDatabaseFolderExists()
        {
            Directory.CreateDirectory(
                DatabaseFolder);
        }
    }
}