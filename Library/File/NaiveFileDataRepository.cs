using System;
using System.IO;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Library.Interface.File;

namespace Library.File
{
    public class NaiveFileDataRepository: IFileDataRepository
    {
        public long GetByteCount(string fileName)
        {
            return new FileInfo(fileName).Length;
        }

        public async Task<FileReadResult> GetPage(string fileName, 
            int pageSize, 
            long pageIndex, 
            CancellationToken cancellationToken = default)
        {
            var offset = pageIndex * pageSize;
            var buffer = new byte[pageSize];
            try
            {
                using (var fileStream = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.Read, FileShare.ReadWrite))
                {
                    try
                    {                        
                        fileStream.Seek(offset, SeekOrigin.Begin);                        
                        await fileStream.ReadAsync(buffer, 0, pageSize, cancellationToken);
                        return new FileReadResult(buffer);
                    }
                    catch (Exception writeException)
                    {
                        return new FileReadResult(writeException, "Error writing to the file");
                    }                    
                }
            }
            catch (Exception openException)
            {
                return new FileReadResult(openException, "Error opening the file for write");
            }     
        }

        public async Task<FileWriteResult> SetPage(string fileName, 
            BigInteger startIndex, 
            byte[] payload, 
            CancellationToken cancellationToken = default)
        {
            try
            {
                using (var fileStream = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite))
                {
                    try
                    {
                        fileStream.Seek((long)startIndex, SeekOrigin.Begin);
                        await fileStream.WriteAsync(payload, 0, payload.Length, cancellationToken);
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