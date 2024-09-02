namespace Editor.Utils;

public class StateMachine<T> where T : System.Enum
{
    public bool Locked { get; set; }
    
    public T State { get; set; }
    public T PreviousState { get; private set; }
    
    public static implicit operator T(StateMachine<T> stateMachine) => stateMachine.State;
}
