namespace Shoutr.Contracts.Io
{
    public interface IStreamFactory
    {
        IReader CreateReader(string fileName);
        IWriter CreateWriter(string fileName);  
    }
}