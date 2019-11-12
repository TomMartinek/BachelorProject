using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace BachelorProject.Models
{
    public class SQLVoucherTypeRepository : IVoucherTypeRepository
    {
        private readonly AppDbContext context;

        public SQLVoucherTypeRepository(AppDbContext context)
        {
            this.context = context;
        }

        public VoucherType Add(VoucherType voucherType)
        {
            context.Add(voucherType);
            context.SaveChanges();
            return voucherType;
        }

        public VoucherType Delete(int id)
        {
            VoucherType voucherType = context.VoucherTypes.Find(id);
            if (voucherType != null)
            {
                context.VoucherTypes.Remove(voucherType);
                context.SaveChanges();
            }
            return voucherType;
        }

        public IEnumerable<VoucherType> GetAllVoucherTypes()
        {
            return context.VoucherTypes;
        }

        public VoucherType GetVoucherType(int id)
        {
            return context.VoucherTypes.Find(id);
        }

        public VoucherType Update(VoucherType voucherTypeChanges)
        {
            var voucherType = context.VoucherTypes.Attach(voucherTypeChanges);
            voucherType.State = EntityState.Modified;

            context.SaveChanges();
            return voucherTypeChanges;
        }
    }
}
