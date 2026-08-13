using CSVFile;

namespace ZiziBot.Application.Common.Utils;

public static class CsvUtil
{
    public static string WriteToCsvFile<T>(this IEnumerable<T> rows, string filePath) where T : class, new()
    {
        var stream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
        var writer = new StreamWriter(stream);

        var csvStr = CSV.Serialize(rows);
        writer.Write(csvStr);

        writer.Flush();
        writer.Close();

        return "";
    }
}