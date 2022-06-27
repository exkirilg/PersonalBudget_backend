namespace Server.Authorization;

public class MustBeOperationAuthorHandler: AuthorizationHandler<MustBeOperationAuthorRequirement>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IBudgetOperationsRepository _repository;
    private readonly UserManager<IdentityUser> _userManager;

    public MustBeOperationAuthorHandler(IHttpContextAccessor httpContextAccessor, IBudgetOperationsRepository repository, UserManager<IdentityUser> userManager)
    {
        _httpContextAccessor = httpContextAccessor;
        _repository = repository;
        _userManager = userManager;
    }

    protected async override Task HandleRequirementAsync(AuthorizationHandlerContext context, MustBeOperationAuthorRequirement requirement)
    {
        if (context.User?.Identity?.IsAuthenticated == false)
        {
            context.Fail();
            return;
        }

        var operationId = _httpContextAccessor.HttpContext?.Request.RouteValues["id"];
        int operationIdAsInt = Convert.ToInt32(operationId);

        var operation = await _repository.GetByIdAsync(operationIdAsInt);
        if (operation is null)
        {
            context.Succeed(requirement);
            return;
        }

        var user = await _userManager.GetUserAsync(context.User);
        var userRoles = await _userManager.GetRolesAsync(user);

        if (userRoles.Contains("Admin") == false && operation.AuthorId != user.Id)
        {
            context.Fail();
            return;
        }

        context.Succeed(requirement);
    }
}
