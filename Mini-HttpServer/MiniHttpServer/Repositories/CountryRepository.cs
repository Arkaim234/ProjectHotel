using MiniHttpServer.Model;
using MyORMLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniHttpServer.Repositories
{
    public class CountryRepository : OrmRepositories<Country>
    {
        public CountryRepository(IORMContext ctx)
            : base(ctx, "Countries") { }
    }
}
