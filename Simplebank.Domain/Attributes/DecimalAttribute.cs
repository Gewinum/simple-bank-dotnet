using System.ComponentModel.DataAnnotations.Schema;

namespace Simplebank.Domain.Attributes;

public class DecimalAttribute : ColumnAttribute
{
    public DecimalAttribute(int precision, int scale)
    {
        TypeName = "decimal(" + precision + "," + scale + ")";
    }
}