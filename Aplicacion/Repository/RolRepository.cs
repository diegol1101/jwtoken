using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dominio.Entities;
using Dominio.Interfaces;
using Microsoft.EntityFrameworkCore;
using Persistencia;

namespace Aplicacion.Repository;
public class RolRepository :GenericRepository<Rol>, IRol
{
    private readonly DbAppContext _context;
    public RolRepository(DbAppContext context):base(context)
    {
        _context = context;
    }
}
