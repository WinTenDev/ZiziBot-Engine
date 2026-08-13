using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace ZiziBot.Application.Features.UseCases.User;

public class CreateSessionOtpRequest
{
    public int Otp { get; set; }
}

public class CreateSessionOtpValidator : AbstractValidator<CreateSessionOtpRequest>
{
    public CreateSessionOtpValidator()
    {
        RuleFor(x => x.Otp).NotNull().WithMessage("Otp is required!");
    }
}

public class CreateSessionOtpResponse
{
    public string? AccessToken { get; set; }
    public double AccessExpireIn { get; set; }
    public string? RefreshToken { get; set; }
    public double RefreshExpireIn { get; set; }
}

public class CreateSessionOtpUseCase(
    DataFacade dataFacade,
    CreateSessionOtpValidator validator,
    GenerateAccessTokenUseCase generateAccessTokenUseCase
)
{
    public async Task<ApiResponseBase<CreateSessionOtpResponse>> Handle(CreateSessionOtpRequest request)
    {
        var response = ApiResponse.Create<CreateSessionOtpResponse>();
        await validator.ValidateAsync(request);

        var userOtp = await dataFacade.MongoDb.UserOtp
            .Where(x => x.Otp == request.Otp)
            .Where(x => x.Status == EventStatus.InProgress)
            .FirstOrDefaultAsync();

        if (userOtp == null)
            return response.Unauthorized("Invalid OTP, please try again!");

        var token = await generateAccessTokenUseCase.Handle(userOtp);

        if (userOtp.IsPermanent == false)
            userOtp.Status = EventStatus.Complete;

        await dataFacade.MongoDb.SaveChangesAsync();

        return response.Success("Success", new CreateSessionOtpResponse()
        {
            AccessToken = token.AccessToken,
            AccessExpireIn = token.AccessExpireIn,
        });
    }
}