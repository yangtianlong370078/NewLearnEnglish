namespace LearnEnglish.Models.ErrorHelper
{
    public class InvalidUserException : Exception
    {
        public InvalidUserException(string message) : base(message) { }
    }
}