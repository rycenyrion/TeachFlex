using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using TeachFlex.Models;

namespace TeachFlex.Services
{
    public sealed class SF9TeacherRemarkRow
    {
        public int TermNumber
        {
            get;
            init;
        }

        public string Remark
        {
            get;
            init;
        } = string.Empty;
    }

    public static class SF9TeacherRemarksRegistry
    {
        private static readonly ConditionalWeakTable<
            SF9ExportRequest,
            List<SF9TeacherRemarkRow>> RemarksByRequest =
                new ConditionalWeakTable<
                    SF9ExportRequest,
                    List<SF9TeacherRemarkRow>>();

        public static void Attach(
            SF9ExportRequest request,
            IEnumerable<SF9TeacherRemarkRow> remarks)
        {
            RemarksByRequest.Remove(request);
            RemarksByRequest.Add(
                request,
                remarks
                    .Where(
                        item =>
                            item.TermNumber >= 1 &&
                            item.TermNumber <= 3 &&
                            !string.IsNullOrWhiteSpace(item.Remark))
                    .OrderBy(item => item.TermNumber)
                    .ToList());
        }

        public static string GetRemark(
            SF9ExportRequest request,
            int termNumber)
        {
            if (!RemarksByRequest.TryGetValue(
                    request,
                    out List<SF9TeacherRemarkRow>? remarks))
            {
                return string.Empty;
            }

            return remarks
                       .FirstOrDefault(
                           item =>
                               item.TermNumber == termNumber)?
                       .Remark
                   ?? string.Empty;
        }
    }
}
