using System.Web.Http;
using Authorization.Api.Attributes;
using Authorization.Api.Helpers;
using StructureMap.Attributes;

namespace Authorization.Api.Controllers
{
    public class DeviceController : BaseApiController
    {
        [OAuthClientAuthorizationScope("manage")]
        [HttpPost]
        public int Register(string uuid, string appType, string appVersion)
        {
            //register device. use uuid to match for existing device
            return _DeviceService.Register(uuid, appType, appVersion);
        }

        [OAuthClientAuthorizationScope("manage")]
        [HttpPost]
        public DeviceSettings UpdateInfo(int deviceId, string uuid, string appType, string appVersion, string systemName, string systemVersion, string model, string language, string country)
        {
            //update info and return back any settings for the app
            return _DeviceService.UpdateInfo(deviceId, new DeviceInfo()
            {
                AppType = appType,
                AppVersion = appVersion,
                Country = country,
                Language = language,
                Model = model,
                SystemName = systemName,
                SystemVersion = systemVersion,
                Uuid = uuid
            });
        }

        [OAuthUserAuthorizationScope("manage")]
        [HttpPost]
        public void SetNotificationToken(int deviceId, string token)
        {
            //set device token for remote notification
            _DeviceService.SetNotificationToken(deviceId, token);
        }

        [OAuthClientAuthorizationScope("manage")]
        [HttpPost]
        public void SetUser(int deviceId, int? userId = null)
        {
            //when a user logs into device, map user to device; if you logs out, remove user mapping to device
            _DeviceService.SetUserId(deviceId, userId);
        }
    }
}