using System.Data;
using FluentMigrator.Builders.Create.Table;
using Nop.Core.Domain.Customers;
using Nop.Data.Extensions;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Misc.PersonalCloud.Domain;

namespace Nop.Plugin.Misc.PersonalCloud.Data
{
    public class CustomerDevicesBuilder : NopEntityBuilder<CustomerDevice>
    {
        /// <summary>
        /// Apply entity configuration
        /// </summary>
        /// <param name="table">Create table expression builder</param>
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table.WithColumn(nameof(CustomerDevice.Id)).AsInt32().PrimaryKey().Identity()
                 .WithColumn(nameof(CustomerDevice.CustomerId)).AsInt32().ForeignKey<Customer>(onDelete: Rule.Cascade)
                 .WithColumn(nameof(CustomerDevice.Identity)).AsString(100)
                 .WithColumn(nameof(CustomerDevice.RefreshToken)).AsString(100)
                 .WithColumn(nameof(CustomerDevice.DeviceInfo)).AsString(int.MaxValue);
        }
    }
}
