using sinpe_validator_api.Application.Common.Models;
using Microsoft.AspNetCore.Identity;

namespace sinpe_validator_api.Infrastructure.Identity;

public static class IdentityResultExtensions
{
    public static Result ToApplicationResult(this IdentityResult result)
    {
        return result.Succeeded
            ? Result.Success()
            : Result.Failure(result.Errors.Select(e => e.Description));
    }
}
