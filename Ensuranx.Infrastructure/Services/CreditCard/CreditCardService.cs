using ErrorOr;
using Ensuranx.Application.Contracts.CreditCard;
using Ensuranx.Application.Interfaces;
using Ensuranx.Application.Requests.CreditCard;
using Ensuranx.Application.Response.CreditCard;
using Ensuranx.Common.Constants;
using Ensuranx.Domain.Entities;
using Ensuranx.Infrastructure.DbContext;
using Ensuranx.Infrastructure.Repositories;
using Mapster;
using Microsoft.Extensions.Logging;

namespace Ensuranx.Infrastructure.Services.CreditCard
{
    /// <summary>
    /// actions related to credit card entity
    /// </summary>
    public class CreditCardService : ICreditCardService
    {
        private IUnitOfWork<long> _iunitOfWork;
        private readonly ILogger _logger;

        public CreditCardService(ApplicationDbContext context, ILogger logger)
        {
            _iunitOfWork = new UnitOfWork<long>(context);
            _logger = logger;
        }

        /// <summary>
        /// add new credit card to the system
        /// </summary>
        /// <param name="addCreditCard">credit card details</param>
        /// <param name="userId">user id who is adding</param>
        /// <param name="email">user email who is adding</param>
        /// <returns>returns success message</returns>
        public async Task<ErrorOr<AddCreditCardResponse>> AddCreditCardAsync(AddCreditCard addCreditCard, long userId, string email)
        {
            _logger.LogInformation("AddCreditCardAsync");

            try
            {
                var paymentOptionAll = await _iunitOfWork.Repository<Domain.Entities.PaymentOption>().GetAllAsync();
                if (paymentOptionAll.Any())
                {
                    TypeAdapterConfig.GlobalSettings.Default.MapToConstructor(true);

                    var config = TypeAdapterConfig.GlobalSettings;
                    TypeAdapterConfig<(AddCreditCard addCreditCard, long userId), CreditCardInfo>.NewConfig()
                    .Map(dest => dest.UserInfoId, src => src.userId)
                    .Map(dest => dest, src => src.addCreditCard);

                    CreditCardInfo creditCardInfo = (addCreditCard, userId).Adapt<CreditCardInfo>();
                    creditCardInfo.CreatedBy = email;
                    creditCardInfo.LastModifiedBy = email;
                    creditCardInfo.CreatedDateTime = DateTime.UtcNow;
                    creditCardInfo.LastModifiedDateTime = DateTime.UtcNow;
                    creditCardInfo.CardBrand = addCreditCard.PaymentOptionStr;
                    creditCardInfo.LastFour = addCreditCard.CardNumber.Substring(addCreditCard.CardNumber.Length - 4);



                    creditCardInfo.PaymentOptionId = addCreditCard.PaymentOptionStr == Domain.Enums.Enum.PaymentOption.ApplePay.ToString() ?
                        paymentOptionAll.Where(x => x.Name.ToLower() == Domain.Enums.Enum.PaymentOption.ApplePay.ToString().ToLower()).Select(x => x.Id).FirstOrDefault() :
                        addCreditCard.PaymentOptionStr == Domain.Enums.Enum.PaymentOption.PayPal.ToString() ?
                        paymentOptionAll.Where(x => x.Name.ToLower() == Domain.Enums.Enum.PaymentOption.PayPal.ToString().ToLower()).Select(x => x.Id).FirstOrDefault() :
                        addCreditCard.PaymentOptionStr == Domain.Enums.Enum.PaymentOption.Stripe.ToString() ?
                        paymentOptionAll.Where(x => x.Name.ToLower() == Domain.Enums.Enum.PaymentOption.Stripe.ToString().ToLower()).Select(x => x.Id).FirstOrDefault() :
                        paymentOptionAll.Where(x => x.Name.ToLower() == Domain.Enums.Enum.PaymentOption.Stripe.ToString().ToLower()).Select(x => x.Id).FirstOrDefault();

                    CancellationToken cancellationToken = CancellationToken.None;
                    var result = await _iunitOfWork.Repository<CreditCardInfo>().AddAsync(creditCardInfo);
                    await _iunitOfWork.Commit(cancellationToken);

                    return new AddCreditCardResponse { Message = Constants.APIErrorMessages.CREDIT_CARD_ADDED_SUCESSFULLY };
                }
                else
                {
                    return Domain.Common.Errors.Errors.User.NoPaymentOptionFound;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception Occur {@ex}", ex);
                return Domain.Common.Errors.Errors.User.ExceptionMessage;
            }
        }
    }
}
