namespace CapstoneProject.Application.Commons.DTOs.Gameplay;

public class ValidateSolutionRequest
{
    public Guid MapId { get; set; }
    public string Language { get; set; } = "Blockly";
    public string? AstSpec { get; set; }
    public string? BytecodeSpec { get; set; }
}
