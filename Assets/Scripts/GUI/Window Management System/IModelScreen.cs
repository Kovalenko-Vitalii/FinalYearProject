using UnityEngine;

public interface IModalScreen
{
    GameObject Root { get; }
    bool BlocksGameplay { get; }
    void OnOpen();
    void OnClose();
}
