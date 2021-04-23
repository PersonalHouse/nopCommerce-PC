using Nop.Core;

namespace Nop.Plugin.Misc.PersonalCloud.Domain
{
    public class CustomerDevice : BaseEntity
    {
        public int CustomerId { get; set; }
        public string Identity { get; set; }
        public string RefreshToken { get; set; }
        public string DeviceInfo { get; set; }
    }
}
