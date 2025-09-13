using CareNest_Order.Application.Features.Commands.Create;
using CareNest_Order.Application.Features.Commands.Update;

namespace CareNest_Order.Application.Exceptions.Validators
{
    public class Validate
    {
        /// <summary>
        /// kiểm tra toàn bộ tạo đơn hàng
        /// </summary>
        /// <param name="command"></param>
        public static void ValidateCreate(CreateCommand command)
        {
            //ValidateShopId(command.ShopId);
        }
        /// <summary>
        /// kiểm tra cập nhật đơn hàng
        /// </summary>
        /// <param name="command"></param>
        public static void ValidateUpdate(UpdateCommand command)
        {
        }
        ///// <summary>
        ///// Valiđ tên đơn hàng
        ///// </summary>
        ///// <param name="name"></param>
        ///// <exception cref="BadRequestException"></exception>
        //public static void ValidateName(string? name)
        //{
        //    //-Không được để trống.
        //    if (string.IsNullOrWhiteSpace(name))
        //    {
        //        throw new BadRequestException(MessageConstant.MissingName);
        //    }
        //    //- Giới hạn độ dài(ví dụ 1 - 100 ký tự).
        //    if (name.Length == 0 || name.Length > 100)
        //    {
        //        throw new BadRequestException(MessageConstant.Exceed100CharsName);
        //    }
        //    //- Không chứa ký tự đặc biệt (!@#$^*&<>?)
        //    if (!Regex.IsMatch(name, @"^[a-zA-Z0-9\s]+$"))
        //    {
        //        throw new BadRequestException(MessageConstant.SpecialCharacterName);
        //    }
        //}
        ///// <summary>
        ///// valid id shop
        ///// </summary>
        ///// <param name="id"></param>
        ///// <exception cref="BadRequestException"></exception>
        //public static void ValidateShopId(string? id)
        //{
        //    // id của chủ shop không được trống
        //    if (string.IsNullOrWhiteSpace(id))
        //    {
        //        throw new BadRequestException(MessageConstant.MissingShopId);
        //    }
        //}
        ///// <summary>
        ///// Valid mô tả đơn hàng
        ///// </summary>
        ///// <param name="description"></param>
        ///// <exception cref="BadRequestException"></exception>
        //public static void ValidateDescription(string? description)
        //{
        //    // mô tả có thể trống 
        //    // mô tả không quá  500 ký tự 
        //    if (!string.IsNullOrWhiteSpace(description) && description.Length > 500)
        //    {
        //        throw new BadRequestException(MessageConstant.Exceed500CharsDescription);
        //    }
        //}
        ///// <summary>
        ///// Valiđ tên đơn hàng
        ///// </summary>
        ///// <param name="name"></param>
        ///// <exception cref="BadRequestException"></exception>
        //public static void ValidateName(string? name)
        //{
        //    //-Không được để trống.
        //    if (string.IsNullOrWhiteSpace(name))
        //    {
        //        throw new BadRequestException(MessageConstant.MissingName);
        //    }
        //    //- Giới hạn độ dài(ví dụ 1 - 100 ký tự).
        //    if (name.Length == 0 || name.Length > 100)
        //    {
        //        throw new BadRequestException(MessageConstant.Exceed100CharsName);
        //    }
        //    //- Không chứa ký tự đặc biệt (!@#$^*&<>?)
        //    if (!Regex.IsMatch(name, @"^[a-zA-Z0-9\s]+$"))
        //    {
        //        throw new BadRequestException(MessageConstant.SpecialCharacterName);
        //    }
        //}
        ///// <summary>
        ///// valid id shop
        ///// </summary>
        ///// <param name="id"></param>
        ///// <exception cref="BadRequestException"></exception>
        //public static void ValidateShopId(string? id)
        //{
        //    // id của chủ shop không được trống
        //    if (string.IsNullOrWhiteSpace(id))
        //    {
        //        throw new BadRequestException(MessageConstant.MissingShopId);
        //    }
        //}
        ///// <summary>
        ///// Valid mô tả đơn hàng
        ///// </summary>
        ///// <param name="description"></param>
        ///// <exception cref="BadRequestException"></exception>
        //public static void ValidateDescription(string? description)
        //{
        //    // mô tả có thể trống 
        //    // mô tả không quá  500 ký tự 
        //    if (!string.IsNullOrWhiteSpace(description) && description.Length > 500)
        //    {
        //        throw new BadRequestException(MessageConstant.Exceed500CharsDescription);
        //    }
        //}
    }
}
