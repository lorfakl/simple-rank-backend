using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace simple_rank_backend.Application.Common.Extensions
{
    public static class ControllerBaseExtensions
    {
        public static ActionResult HandleResult<T>( this ControllerBase controller, Result<T> result)
        {
            if (result.IsSuccess)
            {
                // For GET returning a single item or POST returning the created item
                if (EqualityComparer<T>.Default.Equals(result.Value, default(T)) && typeof(T) != typeof(IEnumerable<>)) // Check for default(T) for non-collection types
                {
                    // This case might be for a successful operation that conceptually returns a value but the value is null (e.g. optional find)
                    // Depending on your API philosophy, you might still return Ok(null) or treat it as NotFound.
                    // For simplicity here, if the result is success but value is default, we'll treat it as if it's an OK with that default.
                    // A more specific GetProductById might treat a successful null find differently.
                    return controller.Ok(result.Value);
                }

                return controller.Ok(result.Value);
            }
            return HandleError(controller, result.Error);
        }

        public static ActionResult HandleResult( this ControllerBase controller, Result result) // For operations without a return value (PUT, DELETE)
        {
            if (result.IsSuccess)
            {
                return controller.Ok(new { result.Message });

            }
            return HandleError(controller, result.Error);
        }

        public static ActionResult HandleError(this ControllerBase controller, Error error)
        {
            // You can map specific error codes to status codes
            switch (error.Code)
            {
                case "Error.NotFound":
                    return controller.NotFound(error); // Return the error object in the body
                case "Error.NullValue":
                case "Error.Validation":
                    return controller.BadRequest(error);
                // Add more specific error mappings as needed
                default:
                    if (error.Code.Contains("Product.Name.Required") || error.Code.Contains("Product.Price.Invalid"))
                    {
                        return controller.BadRequest(error); // Validation-like errors
                    }
                    // For unhandled or generic errors
                    return controller.StatusCode(StatusCodes.Status500InternalServerError, error);
            }
        }
    }
}
