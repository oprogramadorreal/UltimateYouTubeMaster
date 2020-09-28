public interface ITimeBody
{
    void RewindStarted();

    void RewindStopped();

    IMemento CreateMemento();

    void RestoreMemento(IMemento o);
}