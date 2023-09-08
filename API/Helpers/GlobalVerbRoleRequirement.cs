using System.Text;
using System.Runtime.Intrinsics.X86;
using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Dominio.Entities;

namespace API.Helpers;

    public class GlobalVerbRoleRequirement: IAuthorizationRequirement
    {
        public bool IsAllowed(string role,string verb)
        {
            if(string.Equals("Administrador",role,StringComparison.OrdinalIgnoreCase))return true;
            if(string.Equals("Gerente",role,StringComparison.OrdinalIgnoreCase))return true;
            if(string.Equals("Empleado",role,StringComparison.OrdinalIgnoreCase) && String.Equals("GET",verb,StringComparison.OrdinalIgnoreCase))
            {
                return true;
            };
            if(string.Equals("Camper",role,StringComparison.OrdinalIgnoreCase) && String.Equals("GET",verb,StringComparison.OrdinalIgnoreCase))
            {
                return true;
            };
            return false;
        }
    }
