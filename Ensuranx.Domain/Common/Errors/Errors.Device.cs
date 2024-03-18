using ErrorOr;

namespace Ensuranx.Domain.Common.Errors
{
    public static partial class Errors
    {
        public static class Device
        {
            public static Error DeviceNotFound => Error.NotFound(code: "User.DeviceNotFound", description: "Device not found");
            public static Error NoLobbyFoundForThisDevice => Error.NotFound(code: "User.NoLobbyFoundForThisDevice", description: "No lobby found for this device");
        }
    }
}
