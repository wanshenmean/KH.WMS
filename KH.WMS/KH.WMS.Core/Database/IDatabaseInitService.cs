using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace KH.WMS.Core.Database
{
    public interface IDatabaseInitService 
    {
        void CreateDatabase(string databaseName = "");

        void CreateTables();

        void InitDatabase();
    }
}
