// See https://aka.ms/new-console-template for more information

using Azure.Data.Tables;
using SunAuto.Extensions.Console;
using System.Threading;

namespace SunAuto.Logging.Console;

internal class LogUtilities(TableClient tableClient)
{
    readonly TableClient Client = tableClient;

    internal async Task RenamePartitionKeysAsync(string? environment, string oldName, string newName, CancellationToken cancellationtoken)
    {
        System.Console.WriteLine($"Renaming {oldName} to {newName}");

        var result = await RenameAsync(oldName, newName, cancellationtoken);

        $"Renamed {result} entries for {oldName}.".InLivingColorLine(ConsoleColor.Magenta);
    }

    private async Task<int> RenameAsync(string application, string newName, CancellationToken cancellationToken)
    {
        var check = true;
        var result = 0;

        while (check)
        {
            var tasks = new List<Task>();
            var output = Client.Query<Entry>($"PartitionKey eq '{application}'", 1000, null, cancellationToken);

            var page = output
                 .AsPages()
                 .FirstOrDefault();

            if (page == null || !page.Values.Any()) break;

            foreach (var value in page.Values)
            {
                var newvalue = new Entry
                {
                    RowKey = Guid.NewGuid().ToString(),
                    PartitionKey = newName,
                    Timestamp = value.Timestamp,
                    Body = value.Body,
                    Level = value.Level,
                    Message = value.Message
                };

                tasks.Add(Client.AddEntityAsync(newvalue, cancellationToken));
                tasks.Add(Client.DeleteEntityAsync(entity: value, cancellationToken: cancellationToken));
            }

            await Task.WhenAll(tasks);
            result += tasks.Count;
        }

        return result;
    }

    internal async Task CleanupOldEntriesAsync(CancellationToken cancellationToken)
    {
        System.Console.WriteLine($"Deleting old entries...");

        var check = true;
        var result = 0;

        while (check)
        {
            var tasks = new List<Task>();
            var time = DateTime.UtcNow - TimeSpan.FromDays(42);
            var output = Client.Query<Entry>($"Timestamp le datetime'{time:yyyy-MM-ddTHH:mm:ssZ}'", 1000, null, cancellationToken);

            var page = output
                 .AsPages()
                 .FirstOrDefault();

            if (page == null || !page.Values.Any()) break;

            foreach (var value in page.Values)
            {
                tasks.Add(Client.DeleteEntityAsync(entity: value, cancellationToken: cancellationToken));
            }

            await Task.WhenAll(tasks);
            result += tasks.Count;
        }

        $"Deleted {result} entries.".InLivingColorLine(ConsoleColor.Magenta);
    }
}