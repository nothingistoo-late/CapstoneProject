namespace CapstoneProject.Application.Commons.DTOs.Gameplay;

public class UpdateMapSolveScoreConfigRequest
{
    public int BaseScore { get; set; }
    public int TimeScore { get; set; }
    public int StepsScore { get; set; }
    public int BlocksScore { get; set; }
}
