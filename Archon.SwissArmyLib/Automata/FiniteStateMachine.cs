using System;
using System.Collections.Generic;

namespace Archon.SwissArmyLib.Automata
{
    public interface IFsmState<T> : IState<FiniteStateMachine<T>, T> { }
    public class FsmState<T> : State<FiniteStateMachine<T>, T>, IFsmState<T> { }

    public class FiniteStateMachine<T>
    {
        /// <summary>
        /// A shared context which all states have access to.
        /// </summary>
        public T Context { get; private set; }

        /// <summary>
        /// The active state.
        /// </summary>
        public IFsmState<T> CurrentState { get; private set; }

        /// <summary>
        /// The previously active state.
        /// </summary>
        public IFsmState<T> PreviousState { get; private set; }

        private readonly Dictionary<Type, IFsmState<T>> _states = new Dictionary<Type, IFsmState<T>>();

        /// <summary>
        /// Creates a new Finite State Machine.
        /// 
        /// If you need control over how the states are created, you can register them manually using <see cref="RegisterState"/>.
        /// If not, then you can freely use <see cref="ChangeStateAuto{TState}"/> which will create the states using their default constructor.
        /// </summary>
        /// <param name="context">A shared context for the states.</param>
        public FiniteStateMachine(T context)
        {
            Context = context;
        }

        /// <summary>
        /// Creates a new Finite State Machine and changes the state to <paramref name="startState"/>.
        /// 
        /// If you need control over how the states are created, you can register them manually using <see cref="RegisterState"/>.
        /// If not, then you can freely use <see cref="ChangeStateAuto{TState}"/> which will create the states using their default constructor.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="startState"></param>
        public FiniteStateMachine(T context, IFsmState<T> startState) : this(context)
        {
            RegisterState(startState);
            ChangeState(startState);
        }

        /// <summary>
        /// Call this every time the machine should update. Eg. every frame.
        /// </summary>
        public void Update(float deltaTime)
        {
            var currentState = CurrentState;

            if (currentState != null)
            {
                currentState.Reason();

                // we only want to update the state if it's still the current one
                if (currentState == CurrentState)
                    CurrentState.Act(deltaTime);
            }
        }

        /// <summary>
        /// Preemptively add a state instance.
        /// Useful if the state doesn't have an empty constructor and therefore cannot be used with ChangeStateAuto.
        /// </summary>
        /// <param name="state">The state to register.</param>
        public void RegisterState(IFsmState<T> state)
        {
            _states[state.GetType()] = state;
        }

        /// <summary>
        /// Checks whether a state type is registered.
        /// </summary>
        /// <param name="stateType">The state type to check.</param>
        /// <returns>True if registered, false otherwise.</returns>
        public bool IsStateRegistered(Type stateType)
        {
            return _states.ContainsKey(stateType);
;       }

        /// <summary>
        /// Generic version of <see cref="IsStateRegistered"/>.
        /// Checks whether a state type is registered.
        /// </summary>
        /// <typeparam name="TState">The state type to check.</typeparam>
        /// <returns>Tru if registered, false otherwise.</returns>
        public bool IsStateRegistered<TState>() where TState : IFsmState<T>
        {
            return _states.ContainsKey(typeof(TState));
        }

        /// <summary>
        /// Changes the active state to the given state type.
        /// If a state of that type isn't already registered, it will automatically create a new instance using the empty constructor.
        /// </summary>
        /// <typeparam name="TState"></typeparam>
        /// <returns></returns>
        public TState ChangeStateAuto<TState>() where TState : IFsmState<T>, new()
        {
            var type = typeof(TState);
            IFsmState<T> state;

            if (!_states.TryGetValue(type, out state))
                _states[type] = state = new TState();

            return ChangeState((TState) state);
        }

        /// <summary>
        /// Changes the active state to the given state type. 
        /// An instance of that type should already had been registered to use this method.
        /// </summary>
        /// <typeparam name="TState"></typeparam>
        /// <returns></returns>
        public TState ChangeState<TState>() where TState : IFsmState<T>
        {
            var type = typeof(TState);
            IFsmState<T> state;

            if (!_states.TryGetValue(type, out state))
                throw new InvalidOperationException(string.Format("A state of type '{0}' is not registered, did you mean to use ChangeStateAuto?", type));

            return ChangeState((TState)state);
        }

        /// <summary>
        /// Changes the active state to a specific state instance.
        /// This will (if not null) also register the state.
        /// </summary>
        /// <typeparam name="TState"></typeparam>
        /// <param name="state"></param>
        /// <returns></returns>
        public TState ChangeState<TState>(TState state) where TState : IFsmState<T>
        {
            if (CurrentState != null)
                CurrentState.End();

            PreviousState = CurrentState;
            CurrentState = state;

            if (CurrentState != null)
            {
                RegisterState(state);
                CurrentState.Machine = this;
                CurrentState.Context = Context;
                CurrentState.Begin();
            }

            return state;
        }
    }
}
