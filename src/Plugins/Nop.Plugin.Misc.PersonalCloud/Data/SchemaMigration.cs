using FluentMigrator;
using Nop.Data.Mapping;
using Nop.Data.Migrations;
using Nop.Plugin.Misc.PersonalCloud.Domain;

namespace Nop.Plugin.Misc.PersonalCloud.Data
{
    [SkipMigrationOnUpdate]
    [NopMigration("2021/04/19 18:41:55:1687541", "Nop.Plugin.Misc.PersonalCloud base schema")]
    public class SchemaMigration : AutoReversingMigration
    {
        protected IMigrationManager _migrationManager;

        public SchemaMigration(IMigrationManager migrationManager)
        {
            _migrationManager = migrationManager;
        }

        public override void Up()
        {
            _migrationManager.BuildTable<CustomerDevice>(Create);

            Create.Index("IX_CustomerDevices_Identity")
                .OnTable(NameCompatibilityManager.GetTableName(typeof(CustomerDevice)))
                .OnColumn(nameof(CustomerDevice.CustomerId)).Ascending()
                .OnColumn(nameof(CustomerDevice.Identity)).Ascending()
                .WithOptions().Unique();
        }
    }
}