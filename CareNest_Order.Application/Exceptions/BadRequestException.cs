namespace CareNest_Order.Application.Exceptions
{
    public class BadRequestException : Exception
    {
        public BadRequestException(string message) : base(message) { }

        //Lỗi List
        public List<string> Errors { get; } = new List<string>();
        public BadRequestException(IEnumerable<string> messages) : base("One or more validation errors occurred.")
        {
            Errors = messages.ToList();
        }
    }
}
