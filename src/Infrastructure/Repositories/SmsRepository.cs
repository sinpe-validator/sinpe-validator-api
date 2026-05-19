using Application.Contracts;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using sinpe_validator_api.Domain.Entities;

namespace Infrastructure.Repositories;

public class SmsRepository : ISmsRepository
{
    private readonly SinpePaymentsDbContext _dbContext;

    public SmsRepository(SinpePaymentsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> ReferenceExistsAsync(string sinpeReference)
    {
        return await _dbContext.ReceivedSms
            .AnyAsync<ReceivedSms>(s => s.SinpeReference == sinpeReference);
    }
}
