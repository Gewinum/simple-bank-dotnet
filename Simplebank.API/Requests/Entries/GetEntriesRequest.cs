using System.ComponentModel.DataAnnotations;

namespace Simplebank.API.Requests.Entries;

public class GetEntriesRequest
{
    [Range(1, int.MaxValue)]
    public int Page { get; set; }

    [Range(5, 100)]
    public int PerPage { get; set; }
}