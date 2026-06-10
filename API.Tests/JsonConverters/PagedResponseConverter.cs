using System.Text.Json;
using System.Text.Json.Serialization;
using CareerHub.Api.DTOs;

namespace API.Tests.JsonConverters;

public class PagedResponseConverter<T> : JsonConverter<PagedResponse<T>>
{
    public override PagedResponse<T>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var jsonDoc = JsonDocument.ParseValue(ref reader);
        var root = jsonDoc.RootElement;

        var data = root.GetProperty("data").Deserialize<IEnumerable<T>>(options) ?? new List<T>();
        var page = root.GetProperty("page").GetInt32();
        var pageSize = root.GetProperty("pageSize").GetInt32();
        var totalCount = root.GetProperty("totalCount").GetInt32();
        var totalPages = root.GetProperty("totalPages").GetInt32();
        var hasNextPage = root.GetProperty("hasNextPage").GetBoolean();
        var hasPreviousPage = root.GetProperty("hasPreviousPage").GetBoolean();

        // Create a new instance using the parameterized constructor
        return new PagedResponse<T>(data, page, pageSize, totalCount, totalPages, hasNextPage, hasPreviousPage);
    }

    public override void Write(Utf8JsonWriter writer, PagedResponse<T> value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value, options);
    }
}