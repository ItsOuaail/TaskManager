using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManager.Domain.Entities;

namespace TaskManager.Application.Interfaces
{
    /// <summary>
    /// Service for JWT token operations
    /// </summary>
    public interface ITokenService
    {
        string GenerateToken(User user);
    }
}
