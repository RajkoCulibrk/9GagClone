using System.Security.Claims;

namespace _9GagClone.Helpers;

public static class RequestHelper
{

    public static int GetUserId(this ClaimsPrincipal user)
    {
        var userIdStr = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(userIdStr, out int userId))
        {
            return userId;
        }
        throw new InvalidOperationException("Invalid user id");
    }

}