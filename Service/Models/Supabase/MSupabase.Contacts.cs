namespace Service.Models;

using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

[Table("favourite_contacts")]
public class MSupabaseContacts : BaseModel
{
    [PrimaryKey("id", shouldInsert: true)]
    public string FId { get; set; }

    [Column("contacts")]
    public List<string>? Contacts { get; set; }
}