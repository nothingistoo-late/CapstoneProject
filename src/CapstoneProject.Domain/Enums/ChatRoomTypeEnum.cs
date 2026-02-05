namespace CapstoneProject.Domain.Enums;

public enum ChatRoomTypeEnum
{
    Private = 0,      // Private 1-1 chat (implicitly created, no name, exactly 2 participants)
    TemporaryGroup = 1 // Temporary competition group chat (can be closed, flexible participants)
}
