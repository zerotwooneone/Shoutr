using System.Numerics;
using Library.Configuration;

namespace Library.File
{
    public class FileDataRepository: IFileDataRepository
    {
        private readonly IConfigurationService _configurationService;

        public FileDataRepository(IConfigurationService configurationService)
        {
            _configurationService = configurationService;
        }

        public long GetByteCount(string fileName)
        {
            throw new System.NotImplementedException();
        }

        public byte[] GetPage(string fileName, uint pageSize, BigInteger pageIndex)
        {
            throw new System.NotImplementedException();
        }
    }
}