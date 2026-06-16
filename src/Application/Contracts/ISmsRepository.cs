using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Contracts;

public interface ISmsRepository
{
    Task<bool> ReferenceExistsAsync(string sinpeReference);
}
