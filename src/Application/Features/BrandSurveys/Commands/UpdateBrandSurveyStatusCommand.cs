using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Ore.Application.Abstractions.Persistence;
using Ore.Application.Common.Models;

namespace Ore.Application.Features.BrandSurveys.Commands;

public sealed record UpdateBrandSurveyStatusCommand(Guid SurveyId, bool IsActive) : IRequest<Result<Guid>>;

public sealed class UpdateBrandSurveyStatusCommandHandler : IRequestHandler<UpdateBrandSurveyStatusCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _dbContext;

    public UpdateBrandSurveyStatusCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<Guid>> Handle(UpdateBrandSurveyStatusCommand request, CancellationToken cancellationToken)
    {
        var survey = await _dbContext.BrandSurveys
            .FirstOrDefaultAsync(s => s.Id == request.SurveyId, cancellationToken);

        if (survey is null)
        {
            return Result<Guid>.Failure("Survey not found");
        }

        if (request.IsActive)
        {
            survey.Activate();
        }
        else
        {
            survey.Deactivate();
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(survey.Id);
    }
}
