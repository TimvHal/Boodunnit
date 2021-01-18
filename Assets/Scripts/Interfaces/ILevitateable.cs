using DefaultNamespace.Enums;

public interface ILevitateable
{
    bool CanRespawnWhenOutOfRange { get; set; }
    LevitationState State { get; set; }
    void Freeze();
    void Release();
    int TimesLevitated { get; set; }
}