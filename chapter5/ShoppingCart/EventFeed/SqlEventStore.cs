using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;

using Dapper;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ShoppingCart.EventFeed
{
    public class SqlEventStore : IEventStore
    {
        private readonly string _connectionString;

        private const string readEventsSql = @"
SELECT *
  FROM shopcart.EventStore
 WHERE ID >= @Start and ID <= @End
";

        private const string writeEventSql = @"
INSERT INTO shopcart.EventStore(Name, OccurredAt, Content)
VALUES (@Name, @OccurredAt, @Content)
";

        public SqlEventStore(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<IEnumerable<Event>> GetEvents(long? firstEventSequenceNumber = null, long? lastEventSequenceNumber = null)
        {
            using (var conn = await GetOpenConnectionAsync().ConfigureAwait(false))
            {
                // Reads EventStore table rows between start and end
                return (await conn.QueryAsync<dynamic>(
                    readEventsSql,
                    new
                    {
                        Start = firstEventSequenceNumber.GetValueOrDefault(0),
                        End = lastEventSequenceNumber.GetValueOrDefault(long.MaxValue)
                    }).ConfigureAwait(false))
                    .Select(row =>
                    {
                        var converter = new ExpandoObjectConverter();
                        var content = JsonConvert.DeserializeObject<ExpandoObject>(row.Content, converter);

                        // Maps EventStore table rows to Event objects
                        return new Event(row.ID, row.OccurredAt, row.Name, content);
                    });
            }
        }

        public async Task Raise(string eventName, object content)
        {
            var jsonContent = JsonConvert.SerializeObject(content);
            using (var conn = await GetOpenConnectionAsync().ConfigureAwait(false))
            {
                // Uses Dapper to execute a simple SQL insert statement
                await conn.ExecuteAsync(
                    writeEventSql,
                    new
                    {
                        Name = eventName,
                        OccurredAt = DateTimeOffset.UtcNow,
                        Content = jsonContent
                    }).ConfigureAwait(false);
            }
        }

        private async Task<IDbConnection> GetOpenConnectionAsync()
        {
            var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync().ConfigureAwait(false);

            return connection;
        }
    }
}