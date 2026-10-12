using Marketing.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Marketing.Services
{
    public interface IRepository
    {
        void Add(SaleDTO sale);
    }
}
