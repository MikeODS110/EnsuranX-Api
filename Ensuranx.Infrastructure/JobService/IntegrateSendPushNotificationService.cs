using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Ensuranx.Application.Contracts.Notification;
using Ensuranx.Application.Contracts.NotificationTo;
using Ensuranx.Application.Contracts.UserDeviceInfo;
using Ensuranx.Application.Interfaces;
using Ensuranx.Application.JobContract;
using Ensuranx.Application.Response.Notification;
using Ensuranx.Common.Constants;
using Ensuranx.Domain.Entities;
using Ensuranx.Infrastructure.DbContext;
using Ensuranx.Infrastructure.Repositories;
using Ensuranx.Infrastructure.Services.Notification;
using Ensuranx.Infrastructure.Services.NotificationTo;
using Ensuranx.Infrastructure.Services.UserDeviceInfo;
using Newtonsoft.Json;
using Serilog;

namespace Ensuranx.Infrastructure.JobService;

public class IntegrateSendPushNotificationService : IIntegrateSendPushNotificationService
{
    public string CreatedBy = "System";
    private ILogger _logger = Log.ForContext<IntegrateSendPushNotificationService>();
    private readonly ApplicationDbContext _context;
    private readonly INotificationsRepository _inotificationsRepository;
    private readonly IUserDeviceInfoService _iuserDeviceInfoService;

    private object IOSfirebaseApp;
    private object AndroidfirebaseApp;
    public IntegrateSendPushNotificationService(ApplicationDbContext _context)
    {
        this._context = _context;
        _inotificationToService = new NotificationToService(_context);
        _inotificationsRepository = new NotificationsRepository(_context);
        _iuserDeviceInfoService = new UserDeviceInfoService(_context,_logger);
    }

    public async Task IntegrateSendPushNotification()
    {
        _logger.Debug($"IntegrateSendPushNotificationService Scheduler Job Start");

        if (_context == null)
            return;

        using (var dbContextTransaction = _context.Database.BeginTransaction())
        {
            try
            {
                int ProcessedCount = 0;
                List<Notifications> toSendNotificationList = new List<Notifications>();
                List<UserDeviceInfo> toSendUserDeviceInfos = new List<UserDeviceInfo>();
                NotificationToJoinModel notificationToJoinModel = new NotificationToJoinModel();
                List<NotificationToJoinModel> notificationToJoinModels = new List<NotificationToJoinModel>();
                List<NotificationToJoinModel> toSendnotificationToJoinModels = new List<NotificationToJoinModel>();
                List<NotificationToJoinModel> successNotificationToService = new List<NotificationToJoinModel>();

                do
                {
                    ProcessedCount += StaticJobsConstants.NumberOfReocrdToFetch;
                    _logger.Information($"ProcessedCount Count {ProcessedCount}");
                    
                    List<NotificationTo> notificationToList = _inotificationToService.GetMobileToListByIsSendFalse();
                    List<long> notificationIdList = new List<long>();
                    List<long> userInfoIdList = new List<long>();

                    foreach (var notificationsTo in notificationToList)
                    {
                        try
                        {
                            notificationsTo.NumberOfTry++;
                            notificationsTo.LastModifiedDateTime = DateTime.UtcNow;
                            notificationsTo.LastModifiedBy = CreatedBy;

                            var hours = (notificationsTo.LastModifiedDateTime - notificationsTo.CreatedDateTime).TotalHours;
                            if (hours >= 12.00)
                            {
                                notificationsTo.IsSend = true;
                            }
                            await _inotificationToService.UpdateNotificationTo(notificationsTo);
                            Task.WaitAll();
                        }
                        catch (Exception ex)
                        {
                            _logger.Error($"Error-----,{Environment.NewLine} Error InnerException: {ex.InnerException}{Environment.NewLine}Error Message: {ex.Message}");
                        }
                    }


                    if (notificationToList.Any())
                    {
                        notificationToList = notificationToList.Where(x => x.IsSend != true).ToList();
                        notificationIdList = notificationToList.Select(x => x.NotificationsId).Distinct().ToList();

                        bool sendNotification = false;
                        toSendNotificationList = await _inotificationsRepository.GetNotificationsListMobileAsync(notificationIdList);
                        userInfoIdList = notificationToList.Select(x => x.UserInfoId.Value).Distinct().ToList();

                        toSendUserDeviceInfos = await _iuserDeviceInfoService.GetUserDeviceInfoList(userInfoIdList);

                        if (toSendNotificationList.Any() && toSendUserDeviceInfos.Any())
                        {
                            _logger.Information($"toSendNotificationList.count {toSendNotificationList.Count} - toSendUserDeviceInfos.Count {toSendUserDeviceInfos.Count}");
                            foreach (var toSendNotification in toSendNotificationList)
                            {
                                try
                                {
                                    _logger.Information($"notificationToList.count {notificationToList.Count}");
                                    var ValuenotificationsTos = notificationToList.Where(x => x.NotificationsId == toSendNotification.Id).ToList();
                                    if (ValuenotificationsTos.Any())
                                    {
                                        _logger.Information($"ValuenotificationsTos.count {ValuenotificationsTos.Count}");
                                        foreach (var valuenotificationsTo in ValuenotificationsTos)
                                        {
                                            try
                                            {
                                                _logger.Information($"toSendUserDeviceInfos.count {toSendUserDeviceInfos.Count}");
                                                var ValuetoSendUserDeviceInfos = toSendUserDeviceInfos.Where(x => x.UserInfoId == valuenotificationsTo.UserInfoId).ToList();
                                                if (ValuetoSendUserDeviceInfos.Any())
                                                {
                                                    _logger.Information($"ValuetoSendUserDeviceInfos.count {ValuetoSendUserDeviceInfos.Count}");
                                                    foreach (var valuetoSendUserDeviceInfo in ValuetoSendUserDeviceInfos)
                                                    {
                                                        try
                                                        {
                                                            notificationToJoinModel = new NotificationToJoinModel();
                                                            notificationToJoinModel.NotificationToId = valuenotificationsTo.Id;
                                                            notificationToJoinModel.UserInfoId = valuenotificationsTo.UserInfoId.Value;
                                                            notificationToJoinModel.Token = valuetoSendUserDeviceInfo.DeviceFCMToken;
                                                            notificationToJoinModel.Platform = valuetoSendUserDeviceInfo.Platform;
                                                            notificationToJoinModel.Title = toSendNotification.NotificationSubject;
                                                            notificationToJoinModel.Body = toSendNotification.NotificationDetail;
                                                            notificationToJoinModel.unseenCount = _inotificationToService.GetCountOfUnseenNotificationsTo(valuenotificationsTo.UserInfoId.Value);
                                                            notificationToJoinModels.Add(notificationToJoinModel);
                                                            sendNotification = true;
                                                        }
                                                        catch (Exception ex)
                                                        {
                                                            _logger.Error($"Error-----,{Environment.NewLine} Error InnerException: {ex.InnerException}{Environment.NewLine}Error Message: {ex.Message}");
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    _logger.Information($"ValuetoSendUserDeviceInfos.count {ValuetoSendUserDeviceInfos.Count}");
                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                _logger.Error($"Error-----,{Environment.NewLine} Error InnerException: {ex.InnerException}{Environment.NewLine}Error Message: {ex.Message}");
                                            }
                                        }
                                    }
                                    else
                                    {
                                        _logger.Information($"ValuenotificationsTos.count {ValuenotificationsTos.Count}");
                                    }
                                }
                                catch (Exception ex)
                                {
                                    _logger.Error($"Error-----,{Environment.NewLine} Error InnerException: {ex.InnerException}{Environment.NewLine}Error Message: {ex.Message}");
                                }
                            }
                        }
                        else
                        {
                            _logger.Information($"toSendNotifications.count = 0");
                        }

                        if (sendNotification)
                        {
                            _logger.Information($"notificationToJoinModels.count {notificationToJoinModels.Count}");

                            toSendnotificationToJoinModels = new List<NotificationToJoinModel>();
                            toSendnotificationToJoinModels = notificationToJoinModels.Where(x => x.Platform == Domain.Enums.Enum.Platform.Ios).ToList();
                            _logger.Information($"Getting notification list for IOS devices");
                            if (toSendnotificationToJoinModels.Count > 0)
                            {
                                _logger.Information($"notificationToJoinModels.count {toSendnotificationToJoinModels.Count}");
                                var ValueSuccessTokensModel = PushNotifications(toSendnotificationToJoinModels, (int)Domain.Enums.Enum.NotificationsTypes.FollowRequest, true);

                                successNotificationToService.AddRange(ValueSuccessTokensModel);
                            }
                            else
                            {
                                _logger.Information($"Not Found notificationToJoinModels.count {toSendnotificationToJoinModels.Count}");
                            }

                            toSendnotificationToJoinModels = new List<NotificationToJoinModel>();
                            toSendnotificationToJoinModels = notificationToJoinModels.Where(x => x.Platform == Domain.Enums.Enum.Platform.Android).ToList();
                            _logger.Information($"Getting notification list for Android devices");
                            if (toSendnotificationToJoinModels.Count > 0)
                            {
                                _logger.Information($"notificationToJoinModels.count {toSendnotificationToJoinModels.Count}");
                                var ValueSuccessTokensModel = PushNotificationsAndroid(toSendnotificationToJoinModels, (int)Domain.Enums.Enum.NotificationsTypes.FollowRequest, false);

                                successNotificationToService.AddRange(ValueSuccessTokensModel);
                            }
                            else
                            {
                                _logger.Information($"Not Found notificationToJoinModels.count {toSendnotificationToJoinModels.Count}");
                            }

                            _logger.Information($"successNotificationToService.Count: {successNotificationToService.Count}");
                            if (successNotificationToService.Any())
                            {
                                foreach (var valuesuccessNotificationToService in successNotificationToService)
                                {
                                    try
                                    {
                                        _logger.Information($"finding valuesuccessNotificationToService.NotificationToId: {valuesuccessNotificationToService.NotificationToId}");
                                        var ValueNotificationsTos = notificationToList.Where(x => x.Id == valuesuccessNotificationToService.NotificationToId).FirstOrDefault();
                                        if (ValueNotificationsTos != null)
                                        {
                                            _logger.Information($"making valueNotificationsTos.Id: {ValueNotificationsTos.Id} isSend true");
                                            ValueNotificationsTos.IsSend = true;
                                            ValueNotificationsTos.LastModifiedDateTime = DateTime.UtcNow;
                                            ValueNotificationsTos.LastModifiedBy = CreatedBy;

                                            _logger.Information($"updating ValueNotificationsTos.Id: {ValueNotificationsTos.Id}");
                                            var Created = _inotificationToService.UpdateNotificationToWithoutAsync(ValueNotificationsTos);
                                            _logger.Information($"ValueNotificationsTos.Id: {ValueNotificationsTos.Id} updated");

                                        }
                                        else
                                        {
                                            _logger.Information($"valuesuccessNotificationToService.NotificationToId: {valuesuccessNotificationToService.NotificationToId} Not Found");
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        _logger.Error($"Error-----,{Environment.NewLine} Error InnerException: {ex.InnerException}{Environment.NewLine}Error Message: {ex.Message}");
                                    }
                                }
                            }
                            else
                            {
                                _logger.Information($"Not Found successNotificationToService.count {successNotificationToService.Count}");
                            }
                        }
                    }
                    else
                    {
                        _logger.Information($"NO Email to Send notificationToList Count = 0");
                        break;
                    }
                } while (ProcessedCount <= StaticJobsConstants.TotalToProcess);

                dbContextTransaction.Commit();
                Dispose();
            }
            catch (Exception ex)
            {
                _logger.Error($"Error-----,{Environment.NewLine} Error InnerException: {ex.InnerException}{Environment.NewLine} Error Message: {ex.Message}");
            }
        }
        _logger.Debug($"IntegrateSendPushNotificationService Scheduler Job End");
    }


    public List<NotificationToJoinModel> PushNotifications(List<NotificationToJoinModel> notificationToJoinModels, int notificationType, bool IsIOSOrAndroid)
    {
        _logger.Debug($"PushNotifications for notificationToJoinModels {notificationToJoinModels} - notificationType {notificationType} - IsIOSOrAndroid {IsIOSOrAndroid}");

        try
        {
            NotificationReturnModel notificationReturnModel = new NotificationReturnModel();

            string IOSgoogleCredentialJson = StaticJobsConstants.UIauthorizationForIOS;
            string AndroidgoogleCredentialJson = StaticJobsConstants.UIauthorizationForAndroid;

            if (IsIOSOrAndroid)
            {
                if (FirebaseApp.GetInstance("IOS") == null)
                {
                    IOSfirebaseApp = FirebaseApp.Create(new AppOptions()
                    {
                        //Credential =GoogleCredential
                        Credential = GoogleCredential.FromFile(IOSgoogleCredentialJson),
                    }, "IOS");
                }
            }

            if (!IsIOSOrAndroid)
            {
                if (FirebaseApp.GetInstance("Android") == null)
                {
                    AndroidfirebaseApp = FirebaseApp.Create(new AppOptions()
                    {
                        Credential = GoogleCredential.FromFile(AndroidgoogleCredentialJson),
                    }, "Android");
                }
            }

            var Messages = new List<Message>();
            Dictionary<string, string> d2 = new Dictionary<string, string>();
            d2.Add("apns-priority", "5");
            Dictionary<string, string> d = new Dictionary<string, string>();
            d.Add("Urgency", "high");

            List<NotificationToJoinModel> successTokenModel = new List<NotificationToJoinModel>();
            foreach (var notificationToJoinModel in notificationToJoinModels)
            {
                try
                {
                    notificationReturnModel = new NotificationReturnModel();
                    notificationReturnModel.NotificationType = notificationType;
                    notificationReturnModel.NotificationId = notificationToJoinModel.NotificationToId;

                    var Message = new Message();

                    _logger.Information($"notificationToJoinModel.Token {notificationToJoinModel.Token}");
                    _logger.Information($"notificationToJoinModel.Title {notificationToJoinModel.Title}");

                    Message.Token = notificationToJoinModel.Token;
                    Message.Android = new AndroidConfig();
                    Message.Android.Priority = Priority.Normal;

                    Message.Apns = new ApnsConfig();
                    Message.Apns.Headers = new Dictionary<string, string>();
                    Message.Apns.Headers = d2;

                    Message.Webpush = new WebpushConfig();
                    Message.Webpush.Headers = new Dictionary<string, string>();
                    Message.Webpush.Headers = d;

                    Aps Aps = new Aps()
                    {
                        Alert = new ApsAlert()
                        {
                            Title = notificationToJoinModel.Title,
                            Body = notificationToJoinModel.Body
                        },
                        Badge = notificationToJoinModel.unseenCount
                    };
                    Dictionary<string, object> Data = new Dictionary<string, object>();
                    Data.Add("Object", notificationReturnModel);

                    Message.Apns = new ApnsConfig();
                    Message.Apns.Aps = Aps;
                    Message.Apns.CustomData = Data;

                    successTokenModel.Add(notificationToJoinModel);
                    Messages.Add(Message);
                }
                catch (Exception ex)
                {
                    _logger.Error($"Error-----,{Environment.NewLine} Error InnerException: {ex.InnerException}{Environment.NewLine}Error Message: {ex.Message}");
                }
            }

            BatchResponse response;

            _logger.Information($"Messages.Count {Messages.Count}");

            if (IsIOSOrAndroid)
            {
                response = FirebaseMessaging.GetMessaging(FirebaseApp.GetInstance("IOS")).SendAllAsync(Messages).Result;
            }
            else
            {
                response = FirebaseMessaging.GetMessaging(FirebaseApp.GetInstance("Android")).SendAllAsync(Messages).Result;
            }

            return successTokenModel;
        }
        catch (Exception ex)
        {
            _logger.Error($"Error-----,{Environment.NewLine} Error InnerException: {ex.InnerException}{Environment.NewLine}Error Message: {ex.Message}");
        }
        return null;
    }

    public List<NotificationToJoinModel> PushNotificationsAndroid(List<NotificationToJoinModel> notificationToJoinModels, int notificationType, bool IsIOSOrAndroid)
    {
        _logger.Information($"PushNotifications for notificationToJoinModels {notificationToJoinModels} - notificationType {notificationType} - IsIOSOrAndroid {IsIOSOrAndroid}");

        try
        {
            NotificationReturnModel notificationReturnModel = new NotificationReturnModel();

            string IOSgoogleCredentialJson = StaticJobsConstants.UIauthorizationForIOS;
            string AndroidgoogleCredentialJson = StaticJobsConstants.UIauthorizationForAndroid;


            if (IsIOSOrAndroid)
            {
                if (FirebaseApp.GetInstance("IOS") == null)
                {
                    IOSfirebaseApp = FirebaseApp.Create(new AppOptions()
                    {
                        //Credential =GoogleCredential
                        Credential = GoogleCredential.FromFile(IOSgoogleCredentialJson),
                    }, "IOS");
                }
            }

            if (!IsIOSOrAndroid)
            {
                if (FirebaseApp.GetInstance("Android") == null)
                {
                    AndroidfirebaseApp = FirebaseApp.Create(new AppOptions()
                    {
                        Credential = GoogleCredential.FromFile(AndroidgoogleCredentialJson),
                    }, "Android");
                }
            }

            var Messages = new List<Message>();

            List<NotificationToJoinModel> successTokenModel = new List<NotificationToJoinModel>();
            foreach (var notificationToJoinModel in notificationToJoinModels)
            {
                try
                {
                    notificationReturnModel = new NotificationReturnModel();
                    notificationReturnModel.NotificationType = notificationType;
                    notificationReturnModel.NotificationId = notificationToJoinModel.NotificationToId;

                    string jsonData = JsonConvert.SerializeObject(notificationReturnModel);

                    Aps Aps = new Aps()
                    {
                        Badge = notificationToJoinModel.unseenCount
                    };

                    Message message = new Message
                    {
                        Token = notificationToJoinModel.Token,
                        Notification = new Notification
                        {
                            Title = notificationToJoinModel.Title,
                            Body = notificationToJoinModel.Body
                        },
                        Apns = new ApnsConfig
                        {
                            Aps = Aps
                        },
                        Data = new Dictionary<string, string>
                            {
                                { "custom_data", jsonData }
                            }
                    };

                    successTokenModel.Add(notificationToJoinModel);
                    Messages.Add(message);
                }
                catch (Exception ex)
                {
                    _logger.Error($"Error-----,{Environment.NewLine} Error InnerException: {ex.InnerException}{Environment.NewLine}Error Message: {ex.Message}");
                }
            }

            BatchResponse response;

            _logger.Information($"Messages.Count {Messages.Count}");

            if (IsIOSOrAndroid)
            {
                response = FirebaseMessaging.GetMessaging(FirebaseApp.GetInstance("IOS")).SendAllAsync(Messages).Result;
            }
            else
            {
                response = FirebaseMessaging.GetMessaging(FirebaseApp.GetInstance("Android")).SendAllAsync(Messages).Result;
            }

            return successTokenModel;
        }
        catch (Exception ex)
        {
            _logger.Error($"Error-----,{Environment.NewLine} Error InnerException: {ex.InnerException}{Environment.NewLine}Error Message: {ex.Message}");
        }
        return null;
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
