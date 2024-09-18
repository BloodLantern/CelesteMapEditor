using System;
using Microsoft.Xna.Framework;

namespace Editor.Utils;

public class StateMachine<T> where T : struct, Enum
{
    public bool Locked { get; set; } = false;

    private T state = default;

    public T State
    {
        get => state;
        set
        {
            if (Locked)
                return;
            
            ForceState(value);
        }
    }
    public T PreviousState { get; private set; } = default;
    
    public int StateIntegral => Convert.ToInt32(State);
    public int PreviousStateIntegral => Convert.ToInt32(PreviousState);
    public int StateCount => Enum.GetValues<T>().Length;

    public bool StateChanged { get; private set; } = false;

    private Action[] Updates { get; }
    private Action[] Begins { get; }
    private Action[] Ends { get; }
    private Coroutine.Action[] Coroutines { get; }
    
    private Coroutine CurrentCoroutine { get; set; }

    public StateMachine()
    {
        Updates = new Action[StateCount];
        Begins = new Action[StateCount];
        Ends = new Action[StateCount];
        Coroutines = new Coroutine.Action[StateCount];
    }

    public void SetCallbacks(T state, Action update = null, Action begin = null, Action end = null, Coroutine.Action coroutine = null)
    {
        int stateIndex = StateIntegral;
        
        Updates[stateIndex] = update;
        Begins[stateIndex] = begin;
        Ends[stateIndex] = end;
        Coroutines[stateIndex] = coroutine;
    }

    public void Update(GameTime time)
    {
        int stateIndex = StateIntegral;
        
        if (StateChanged)
        {
            Begins[stateIndex]?.Invoke();

            Coroutine.Action coroutineAction = Coroutines[stateIndex];
            if (coroutineAction != null)
                CurrentCoroutine = new(coroutineAction());
            else
                CurrentCoroutine = null;
            
            StateChanged = false;
        }
        
        Updates[stateIndex]?.Invoke();
        
        if (StateChanged)
            Ends[stateIndex]?.Invoke();
        
        if (CurrentCoroutine is { Finished: false })
            CurrentCoroutine.Update(time);
    }

    public void ForceState(T state)
    {
        PreviousState = State;
        State = state;
        StateChanged = true;
    }
}
