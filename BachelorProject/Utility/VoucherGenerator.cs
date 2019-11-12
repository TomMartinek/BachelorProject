using BachelorProject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BachelorProject.Utility
{
    public static class VoucherGenerator
    {
        public static string GetHTMLString(Voucher voucher)
        {
            var sb = new StringBuilder();
            sb.Append(@"
                        <html>
                            <head>
                            </head>
                            <body>");
            sb.AppendFormat(@"
                                <h1>{0}</h1>
                                <h2>{1}</h2>", voucher.Title, voucher.Description);

            sb.AppendFormat(@"
                                <table align='center'>
                                    <tr>
                                        <th>Platný od:</th>
                                        <td>{0}</td>
                                        <th>Celková hodnota:</th>
                                        <td>{2},- kč</td>
                                    </tr>
                                    <tr>
                                        <th>Platný do:</th>
                                        <td>{1}</td>
                                        <th>Kód:</th>
                                        <td>{3}</td>
                                     </tr>
                                </table>
                            </html>", voucher.ValidFrom.ToShortDateString(), voucher.ValidUntil.ToShortDateString(), voucher.Value, voucher.Code);

            return sb.ToString();
        }
    }
}
