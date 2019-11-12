using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BachelorProject.Models
{
    public interface IVoucherRepository
    {
        Voucher GetVoucher(int id);
        IEnumerable<VoucherType> GetAllVoucherTypes();
        IEnumerable<Voucher> GetAllVouchers();
        IQueryable<Voucher> GetAllVouchersAsIQueriable();

        Voucher Add(Voucher voucher);
        Voucher Update(Voucher voucherChanges);
        Voucher Delete(int id);

        Voucher FindVoucherByCode(string code);
    }
}
