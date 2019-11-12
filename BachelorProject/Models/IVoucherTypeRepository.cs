using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BachelorProject.Models
{
    public interface IVoucherTypeRepository
    {
        VoucherType GetVoucherType(int id);
        IEnumerable<VoucherType> GetAllVoucherTypes();
        VoucherType Add(VoucherType voucherType);
        VoucherType Update(VoucherType voucherTypeChanges);
        VoucherType Delete(int id);
    }
}
