using System;

namespace Library.File
{
    public class FileReadResult
    {
        public Exception Exception {get;}
        public bool Success {get;}
        public byte[] Bytes { get; }
        public string TechnicalErrorMessage {get;}

        public FileReadResult(byte[] bytes)
        {
            Exception = null;
            Bytes = bytes ?? throw new ArgumentNullException(nameof(bytes));
            TechnicalErrorMessage = null;
            Success = true;
        }

        public FileReadResult(string technicalErrorMessage)
        {
            Exception = null;
            Bytes = null;
            TechnicalErrorMessage = technicalErrorMessage ?? throw new ArgumentNullException(nameof(technicalErrorMessage));
            Success = false;
        }

        public FileReadResult(Exception exception, 
            string technicalErrorMessage = null)
        {
            Exception = exception ?? throw new ArgumentNullException(nameof(exception));
            TechnicalErrorMessage = technicalErrorMessage ?? exception.ToString();
            Success = false;
            Bytes = null;
        }
    }
}