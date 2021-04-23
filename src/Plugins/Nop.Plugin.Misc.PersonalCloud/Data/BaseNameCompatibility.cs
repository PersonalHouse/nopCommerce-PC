using System;
using System.Collections.Generic;
using Nop.Data.Mapping;
using Nop.Plugin.Misc.PersonalCloud.Domain;

namespace Nop.Plugin.Misc.PersonalCloud.Data
{
    public partial class BaseNameCompatibility : INameCompatibility
    {
        public Dictionary<Type, string> TableNames => new Dictionary<Type, string>
        {
            { typeof(CustomerDevice), "CustomerDevices" },
        };

        public Dictionary<(Type, string), string> ColumnName => new Dictionary<(Type, string), string>
        {
        };
    }
}