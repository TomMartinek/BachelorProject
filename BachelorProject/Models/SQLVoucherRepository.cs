using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace BachelorProject.Models
{
    // implementace rozhraní "IVoucherRepository"
    public class SQLVoucherRepository : IVoucherRepository
    {
        private readonly AppDbContext context;

        public SQLVoucherRepository(AppDbContext context)
        {
            this.context = context;
        }

        public Voucher Add(Voucher voucher)
        {
            context.Add(voucher);
            context.SaveChanges();
            return voucher;
        }

        public Voucher Delete(int id)
        {
            Voucher voucher = context.Vouchers.Find(id);
            if (voucher != null)
            {
                context.Vouchers.Remove(voucher);
                context.SaveChanges();
            }
            return voucher;
        }

        public Voucher FindVoucherByCode(string code)
        {
            return context.Vouchers.Where(v => v.Code == code).FirstOrDefault();
        }

        public IEnumerable<Voucher> GetAllVouchers()
        {
            return context.Vouchers;
        }

        public IQueryable<Voucher> GetAllVouchersAsIQueriable()
        {
            return context.Vouchers.OrderByDescending(v => v.CreationDate);
        }

        //public IEnumerable<VoucherType> GetAllVoucherTypes()
        //{
        //    return context.VoucherTypes;
        //}

        public Voucher GetVoucher(int id)
        {
            return context.Vouchers.Include(u => u.ApplicationUser)
                                   .AsEnumerable()
                                   .FirstOrDefault(v => v.Id == id);
        }

        public Voucher Update(Voucher voucherChanges)
        {
            var voucher = context.Vouchers.Attach(voucherChanges);
            voucher.State = EntityState.Modified;

            context.SaveChanges();
            return voucherChanges;
        }
    }
}
