using System;
using System.IO;
using System.Numerics;
using System.Threading.Tasks;

namespace Library.File
{
    public class NaiveFileDataRepository: IFileDataRepository
    {
        public long GetByteCount(string fileName)
        {
            return new FileInfo(fileName).Length;
        }

        public byte[] GetPage(string fileName, uint pageSize, BigInteger pageIndex)
        {            
            using (var fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {                    
                var startIndex = pageIndex * pageSize;
                fileStream.Seek((long)startIndex, SeekOrigin.Begin);
                var result = new byte[pageSize];
                fileStream.Read(result, 0, (int)pageSize); 
                return result;
            }
        }

        public async Task<FileWriteResult> SetPage(string fileName, BigInteger startIndex, byte[] payload)
        {
            try
            {
                using (var fileStream = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite))
                {
                    try
                    {
                        fileStream.Seek((long)startIndex, SeekOrigin.Begin);
                        await fileStream.WriteAsync(payload, 0, payload.Length);
                    }
                    catch (Exception writeException)
                    {
                        return new FileWriteResult(writeException, "Error writing to the file");
                    }
                    return new FileWriteResult();
                }
            }
            catch (Exception openException)
            {
                return new FileWriteResult(openException, "Error opening the file for write");
            }            
        }
    }
}