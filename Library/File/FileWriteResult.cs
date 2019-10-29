using System;

namespace Library.File
{
    public class FileWriteResult
    {
        public Exception Exception {get;}

        public FileWriteResult(string technicalErrorMessage = null)
        {
            Exception = null;
            TechnicalErrorMessage = technicalErrorMessage;
            Success = string.IsNullOrEmpty(technicalErrorMessage);
        }

        public FileWriteResult(Exception exception, 
            string technicalErrorMessage = null)
        {
            Exception = exception ?? throw new ArgumentNullException(nameof(exception));
            TechnicalErrorMessage = technicalErrorMessage;
            Success = false;
        }

        public bool Success {get;}
        public string TechnicalErrorMessage {get;}
    }
}