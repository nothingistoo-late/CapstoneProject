namespace CapstoneProject.Application.Commons.Models.Xp;

public class XpGrantResult
{
    public bool IsDuplicate { get; set; }
    public int GrantedXp { get; set; }
    public int NewTotalXp { get; set; }
    public int PreviousLevel { get; set; }
    public int NewLevel { get; set; }
    public bool IsLevelUp => NewLevel > PreviousLevel;
    public Guid? TransactionId { get; set; }
}

