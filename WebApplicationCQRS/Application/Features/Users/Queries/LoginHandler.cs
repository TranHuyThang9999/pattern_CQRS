using System.Globalization;
using System.Net;
using MediatR;
using WebApplicationCQRS.Common.Enums;
using WebApplicationCQRS.Domain;
using WebApplicationCQRS.Domain.Interfaces;
using WebApplicationCQRS.Infrastructure.Security;

namespace WebApplicationCQRS.Application.Features.Users.Queries;

public class LoginHandler : IRequestHandler<LoginQuery, Result<string>>
{
    
    private readonly IJwtService _jwtService;
    private readonly IUnitOfWork _unitOfWork;

    public LoginHandler(IJwtService jwtService, IUnitOfWork unitOfWork)
    {
        _jwtService = jwtService;
        _unitOfWork = unitOfWork;
    }
    
    public async Task<Result<string>> Handle(LoginQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _unitOfWork.UserRepository.GetUserByUsername(request.UserName);
            if (user is null)
            {
                return Result<string>.Failure(ResponseCode.Conflict, "User not found", HttpStatusCode.NotFound);
            }

            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
            {
                return Result<string>.Failure(ResponseCode.Conflict, "User not found", HttpStatusCode.NotFound);
            }

            var customClaims = new Dictionary<string, string>
            {
                { "userID", user.Id.ToString() },
                { "lastPasswordUpdate", user.LastPasswordChangedAt.ToString(CultureInfo.InvariantCulture) }
            };

            var token = _jwtService.GenerateJwtToken(user, customClaims);
            return Result<string>.Success(token);
        }
        catch (Exception e)
        {
            return Result<string>.Failure(ResponseCode.InternalError, "Internal Server Error",
                HttpStatusCode.InternalServerError);
        }
    }
}