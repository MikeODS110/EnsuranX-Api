using Azure;
using Ensuranx.Application.Response.BusinessPackage;
using Ensuranx.Domain.Entities;
using Mapster;
using static Ensuranx.Domain.Enums.Enum;

namespace Ensuranx.Infrastructure.Mappers
{
    public class BusinessMapper
    {
        public AllPackagesResponse MappingBusinessPackagesResponse(List<Domain.Entities.PaymentOption> paymentOptions,
            List<BusinessPackages> businessPackageList)
        {
            AllPackagesResponse allPackagesResponse = new AllPackagesResponse();
            allPackagesResponse.PackageDetail = new List<PackageDetail>();

            foreach (var businessPackage in businessPackageList)
            {
                PackageDetail packageDetail = new PackageDetail();
                var splitDescriptionForPoint = businessPackage.Description.Split(',');

                packageDetail = businessPackage.Adapt<PackageDetail>();
                packageDetail.BusinessPackageTypeName = businessPackage.BusinessPackageType.Name;

                if (splitDescriptionForPoint.Length > 0)
                {
                    packageDetail.DescriptionList = new List<string>();
                    foreach (var descArr in splitDescriptionForPoint)
                    {
                        packageDetail.DescriptionList.Add(descArr);
                    }
                }
                allPackagesResponse.PackageDetail.Add(packageDetail); 
            }

            allPackagesResponse.PaymentOption = new List<PaymentOpt>();

            foreach (Domain.Entities.PaymentOption pay in paymentOptions)
            {
                PaymentOpt paymentOpt = new PaymentOpt();
                paymentOpt.Name = pay.Name;
                paymentOpt.Id = pay.Id;
                allPackagesResponse.PaymentOption.Add(paymentOpt);
            }

            return allPackagesResponse;
        }

        public Business MappingBusinessEntity(string name,long businessOwnerId,long businessTypeId, string email)
        {
            Business business = new Business();

            business.Name = name;
            business.BusinessOwnerId = businessOwnerId;
            business.BusinessTypeId = businessTypeId;
            business.CreatedBy = email;
            business.LastModifiedBy = email;
            business.LastModifiedDateTime = DateTime.UtcNow;
            business.CreatedDateTime = DateTime.UtcNow;
            business.Description = string.Empty;

            return business;
        }

        public Business MappingBusinessEntityToUpdate(string name, long businessOwnerId, long businessTypeId, string email, Business business)
        {
            business.Name = name;
            business.BusinessOwnerId = businessOwnerId;
            business.BusinessTypeId = businessTypeId;
            business.LastModifiedBy = email;
            business.LastModifiedDateTime = DateTime.UtcNow;

            return business;
        }
    }
}
