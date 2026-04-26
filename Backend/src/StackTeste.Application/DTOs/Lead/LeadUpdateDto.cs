using StackTeste.Domain.Helpers.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StackTeste.Application.DTOs.Lead
{
    public record LeadUpdateDto(
        string Name, 
        string Email, 
        LeadStatus Status
    );
}
