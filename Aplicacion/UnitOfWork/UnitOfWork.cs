
using Aplicacion.Repository;
using Dominio.Interfaces;
using Persistencia;

namespace Aplicacion.UnitOfWork;

    public class UnitOfWork : IUnitOfWork, IDisposable
    {
        private readonly DbAppContext context;
        private  UserRepository _users;
        private RolRepository _rols;

        public UnitOfWork( DbAppContext _context)
        {
            context = _context;
        }
        public  IUser Users
        {
            get{
                if(_users== null){
                    _users= new (context);
                }
                return _users ;
            }
        }

        public  IRol Rols
        {
            get{
                if(_rols== null){
                    _rols= new (context);
                }
                return _rols ;
            }
        }

        public void Dispose()
        {
            context.Dispose();
        }
        public async Task<int> SaveAsync()
        {
            return await context.SaveChangesAsync();
        }
    }
    
    