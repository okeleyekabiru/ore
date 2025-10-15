using System;
using System.Threading;
using System.Threading.Tasks;
using Ore.Domain.Entities;

namespace Ore.Application.Abstractions.Scheduling;

public interface ISchedulingService
{
    Task ScheduleAsync(ContentDistribution distribution, CancellationToken cancellationToken = default);
    Task CancelAsync(Guid distributionId, CancellationToken cancellationToken = default);
}
