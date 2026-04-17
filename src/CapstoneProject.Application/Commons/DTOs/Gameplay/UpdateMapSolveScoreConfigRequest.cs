namespace CapstoneProject.Application.Commons.DTOs.Gameplay;

public class UpdateGameSolveScoreConfigRequest
{
    public int BaseScore { get; set; }
    public int TimeScore { get; set; }
    public int StepsScore { get; set; }
    public int BlocksScore { get; set; }
}
