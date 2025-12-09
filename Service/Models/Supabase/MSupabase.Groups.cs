namespace Service.Models;

using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

[Table("groups")]
public class MSupabaseGroups : BaseModel
{
    [PrimaryKey("id", shouldInsert: true)]
    public string Id { get; set; }

    [Column("profile_ids")]
    public List<string>? ProfileIds { get; set; }
}