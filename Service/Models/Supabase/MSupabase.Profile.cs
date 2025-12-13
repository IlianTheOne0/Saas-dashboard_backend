namespace Service.Models;

using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

[Table("profiles")]
public class MSupabaseProfile : BaseModel
{
    [PrimaryKey("id", shouldInsert: true)]
    public string FId { get; set; }

    [Column("name")]
    public string? Name { get; set; }
    [Column("username")]
    public string? Username { get; set; }

    [Column("avatar_url")]
    public string? AvatarUrl { get; set; }
    [Column("role")]
    public string? Role { get; set; }
    [Column("group_id")]
    public string? FGroupId { get; set; }
    [Column("is_online")]
    public bool? IsOnline { get; set; }
    [Column("location")]
    public string? Location { get; set; }
    [Column("timezone")]
    public string? Timezone { get; set; }
    [Column("phone")]
    public string? Phone { get; set; }
}