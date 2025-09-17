namespace CareNest_Order.Domain.Commons.Constant
{
    public class MessageConstant
    {
        public const string NotFound = "The requested resource was not found.";
        public const string InvalidRequest = "The request is invalid.";
        public const string Unauthorized = "You are not authorized to perform this action.";
        public const string InternalServerError = "An unexpected error occurred. Please try again later.";
        public const string SuccessGet = "Get successfully.";
        public const string SuccessCreate = "Created successfully.";
        public const string SuccessUpdate = "Updated successfully.";
        public const string SuccessDelete = "Deleted successfully.";
        public const string DuplicateRecord = "A record with the same key already exists.";
        public const string ValidationFailed = "Data validation failed.";
        public const string OperationFailed = "The operation could not be completed.";
        //paymentMethod 
        public const string MissingPaymentMethod = "Payment method cannot empty.";

        public const string WrongFormatField = "The field is wrong format.";
        public const string NoPermissionCompany = "You don't have permission to interact with this company";
    }
}
