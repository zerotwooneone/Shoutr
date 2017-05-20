using Library.Configuration;

namespace Library.File
{
    public class FileDataFactory: IFileDataFactory
    {
        private readonly IConfigurationService _configurationService;

        public FileDataFactory(IConfigurationService configurationService)
        {
            _configurationService = configurationService;
        }

        public FileData Create(string fileName)
        {
            throw new System.NotImplementedException();
        }
    }
}