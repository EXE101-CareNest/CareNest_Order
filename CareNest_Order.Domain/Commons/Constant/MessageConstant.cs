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
        //name
        public const string MissingName = "Name must not be empty or null.";
        public const string Exceed100CharsName = "Name must not exceed 100 characters.";
        public const string SpecialCharacterName = "Name must not contain special characters (!@#$^*&<>?).";

        // shopId
        public const string MissingShopId = "ShopId must not be empty or null.";

        // description
        public const string Exceed500CharsDescription = "Description must not exceed 500 characters.";

        public const string WrongFormatField = "The field is wrong format.";
        public const string NoPermissionCompany = "You don't have permission to interact with this company";
    }
}
