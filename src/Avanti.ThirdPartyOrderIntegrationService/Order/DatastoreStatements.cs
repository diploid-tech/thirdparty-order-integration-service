using System.Data;
using Npgsql;
using NpgsqlTypes;

namespace Avanti.ThirdPartyOrderIntegrationService.Order
{
    public static class DataStoreStatements
    {
        private const string Schema = "\"order\"";
        private const string OrderTable = "\"order\"";

        public static string GetOrderHashByExternalId => $@"
            SELECT hash FROM {Schema}.{OrderTable}
            WHERE externalId = @ExternalId
        ";

        public static string InsertOrderHash => $@"
            INSERT INTO {Schema}.{OrderTable} (externalId, hash, created)
            VALUES (@ExternalId, @Hash, @Now)
        ";
    }
}
