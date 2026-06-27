using Foodics.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace Foodics.Filters
{
    public class ActiveUserFilter : IAsyncAuthorizationFilter
    {
        private readonly UserManager<User> _userManager;

        public ActiveUserFilter(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            // لو الـ Endpoint مش محتاج Authorize
            var hasAuthorize = context.ActionDescriptor.EndpointMetadata
                .OfType<AuthorizeAttribute>()
                .Any();

            if (!hasAuthorize)
                return;

            // AllowAnonymous
            var allowAnonymous = context.ActionDescriptor.EndpointMetadata
                .OfType<AllowAnonymousAttribute>()
                .Any();

            if (allowAnonymous)
                return;

            // المستخدم مش عامل Login
            if (context.HttpContext.User.Identity?.IsAuthenticated != true)
                return;

            var userId = context.HttpContext.User.FindFirstValue("userId");

            if (string.IsNullOrEmpty(userId))
            {
                context.Result = new UnauthorizedObjectResult(new
                {
                    message = "User not found."
                });

                return;
            }

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                context.Result = new UnauthorizedObjectResult(new
                {
                    message = "User not found."
                });

                return;
            }

            if (user.IsDeleted)
            {
                context.Result = new ObjectResult(new
                {
                    message = "Your account has been deleted."
                })
                {
                    StatusCode = StatusCodes.Status403Forbidden
                };

                return;
            }

            if (user.IsBlocked)
            {
                context.Result = new ObjectResult(new
                {
                    message = "Your account has been blocked."
                })
                {
                    StatusCode = StatusCodes.Status403Forbidden
                };

                return;
            }
        }
    }
}