namespace Service.Models;

using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

[Table("profiles")]
public class MSupabaseProfile : BaseModel
{
    [PrimaryKey("id", shouldInsert: true)]
    public string Id { get; set; }

    [Column("name")]
    public string? Name { get; set; }

    [Column("avatar_url")]
    public string? AvatarUrl { get; set; }
}