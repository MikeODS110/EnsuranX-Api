using static Ensuranx.Domain.Enums.Enum;

namespace Ensuranx.Application.Response.BusinessPackage
{
    public class AllPackagesResponse
    {
        public List<PackageDetail> PackageDetail { get; set; }
        public List<PaymentOpt> PaymentOption { get; set; }
    }

    public class PackageDetail
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public long BusinessPackageTypeId { get; set; }
        public string BusinessPackageTypeName { get; set; }
        public decimal Price { get; set; }
        public List<string> DescriptionList { get; set; }
    }

    public class PaymentOpt
    {
        public long Id { get; set; }
        public string Name { get; set; }
    }
}
