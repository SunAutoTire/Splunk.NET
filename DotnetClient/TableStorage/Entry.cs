namespace SunAuto.Logging.Client.TableStorage;

public class Entry 
{
   public string Application { get; set; } = null!;

   public string? Message { get; set; }
   public string Level { get; set; } = null!;
   public object? Body { get; set; } 

   /// <summary>
   /// AKA ApplicationName
   /// </summary>
   public string RowKey { get; set; } =null!;
   public DateTimeOffset? Timestamp { get; set; }
}