using System;
using System.Collections.Generic;
using Archon.SwissArmyLib.Pooling;

namespace Archon.SwissArmyLib.Automata
{
    /// <summary>
    /// A simple <see href="https://en.wikipedia.org/wiki/Pushdown_automaton">Pushdown Automaton</see> with states as objects.
    /// 
    /// If your state classes have an empty constructor, the state machine can register the states automatically when needed (using <see cref="PushStateAuto{TState}"/> and <see cref="ChangeStateAuto{TState}"/>).
    /// If not you should register the states yourself using <see cref="RegisterStateType{TState}"/> or <see cref="RegisterStateType"/> and use the regular.
    /// 
    /// The machine will automatically pool the states so you don't have to worry about it.
    ///
    /// Whether or popping the last state is valid is up to your design.
    /// 
    /// <seealso cref="IPdaState{T}"/>
    /// <seealso cref="PdaState{T}"/>
    /// </summary>
    /// <typeparam name="T">The type of the context.</typeparam>
    public class PushdownAutomaton<T>
    {
        /// <summary>
        /// A shared context which all states have access to.
        /// </summary>
        public T Context { get; private set; }
        
        /// <summary>
        /// The active state.
        /// </summary>
        public IPdaState<T> CurrentState
        {
            get
            {
                return _stateStack.Count > 0 ? _stateStack.Peek() : null;
            }
        }

        private readonly Stack<IPdaState<T>> _stateStack = new Stack<IPdaState<T>>();
        private readonly Dictionary<Type, Pool<IPdaState<T>>> _statePools = new Dictionary<Type, Pool<IPdaState<T>>>();

        /// <summary>
        /// Creates a new PushdownAutomaton.
        /// 
        /// You should use <see cref="RegisterStateType"/> to register which state types that can be used with the machine.
        /// </summary>
        /// <param name="context">Data shared among states</param>
        public PushdownAutomaton(T context)
        {
            Context = context;
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
                    currentState.Act(deltaTime);
            }
        }

        /// <summary>
        /// Replaces the active state with another state, without notifying the underlying state.
        /// </summary>
        /// <typeparam name="TState">The type of the state to change to.</typeparam>
        /// <returns>The state instance that was changed to.</returns>
        public TState ChangeState<TState>() where TState : IPdaState<T>
        {
            PopStateSilently();
            return PushStateSilently<TState>();
        }


        /// <summary>
        /// Replaces the active state with another state, without notifying the underlying state.
        /// 
        /// If the state is not registered, it will automatically be and its empty constructor will be used to create the instances.
        /// 
        /// <seealso cref="ChangeState{TState}"/>
        /// </summary>
        /// <typeparam name="TState">The type of the state to change to.</typeparam>
        /// <returns>The state instance that was changed to.</returns>
        public TState ChangeStateAuto<TState>() where TState : IPdaState<T>, new()
        {
            if (!IsRegistered<TState>())
                RegisterStateType<TState>();

            return ChangeState<TState>();
        }

        /// <summary>
        /// Pops the current state and resumes the underlying state.
        /// </summary>
        public void PopState()
        {
            PopStateSilently();

            if (CurrentState != null)
                CurrentState.Resume();
        }

        /// <summary>
        /// Pops the current state without notifying the underlying state.
        /// </summary>
        private void PopStateSilently()
        {
            var poppedState = _stateStack.Pop();
            poppedState.End();

            FreeState(poppedState);
        }

        /// <summary>
        /// Pops all states in the stack.
        /// </summary>
        /// <param name="excludingRoot">Whether to keep the bottom state.</param>
        public void PopAll(bool excludingRoot = false)
        {
            var targetCount = excludingRoot ? 1 : 0;
            while (_stateStack.Count > targetCount)
                PopState();
        }

        /// <summary>
        /// Pushes a state to the top of the stack and pauses the underlying state.
        /// </summary>
        /// <typeparam name="TState">The type of the state to change to.</typeparam>
        /// <returns>The new state.</returns>
        public TState PushState<TState>() where TState : IPdaState<T>
        {
            if (CurrentState != null)
                CurrentState.Pause();

            return PushStateSilently<TState>();
        }

        /// <summary>
        /// Pushes a state to the top of the stack without notifying the underlying state.
        /// </summary>
        /// <typeparam name="TState">The type of the state to change to.</typeparam>
        /// <returns>The new state.</returns>
        public TState PushStateSilently<TState>() where TState : IPdaState<T>
        {
            var newState = ObtainState<TState>();
            newState.Machine = this;
            newState.Context = Context;
            _stateStack.Push(newState);
            newState.Begin();

            return newState;
        }

        /// <summary>
        /// Pushes a state to the top of the stack and pauses the underlying state.
        /// 
        /// If the state is not registered, it will automatically be and its empty constructor will be used to create the instances.
        /// 
        /// <seealso cref="PushState{TState}"/>
        /// </summary>
        /// <typeparam name="TState">The type of the state to change to.</typeparam>
        /// <returns>The new state.</returns>
        public TState PushStateAuto<TState>() where TState : IPdaState<T>, new()
        {
            if (CurrentState != null)
                CurrentState.Pause();

            return PushStateSilentlyAuto<TState>();
        }

        /// <summary>
        /// Pushes a state to the top of the stack without notifying the underlying state.
        /// 
        /// If the state is not registered, it will automatically be and its empty constructor will be used to create the instances.
        /// 
        /// <seealso cref="PushStateSilently{TState}"/>
        /// </summary>
        /// <typeparam name="TState">The type of the state to change to.</typeparam>
        /// <returns>The new state.</returns>
        public TState PushStateSilentlyAuto<TState>() where TState : IPdaState<T>, new()
        {
            if (!IsRegistered<TState>())
                RegisterStateType<TState>();

            return PushStateSilently<TState>();
        }

        /// <summary>
        /// Obtains a pooled instance of the given type.
        /// </summary>
        /// <typeparam name="TState">The type of the state to obtain.</typeparam>
        /// <returns>The new or recycled state.</returns>
        private TState ObtainState<TState>() where TState : IPdaState<T>
        {
            var pool = GetPool(typeof(TState));
            return (TState) pool.Spawn();
        }

        /// <summary>
        /// Frees a state instance and makes it available for reuse.
        /// </summary>
        /// <param name="state">The state to free.</param>
        private void FreeState(IPdaState<T> state)
        {
            var type = state.GetType();
            var pool = GetPool(type);
            pool.Despawn(state);
        }

        /// <summary>
        /// Gets a pool for the given state type.
        /// </summary>
        /// <param name="stateType">The state type to get the pool for.</param>
        /// <returns>The pool, or null if not found.</returns>
        private Pool<IPdaState<T>> GetPool(Type stateType)
        {
            Pool<IPdaState<T>> pool;
            _statePools.TryGetValue(stateType, out pool);
            return pool;
        }

        /// <summary>
        /// Registers a state type in the machine. 
        /// 
        /// A pool for the type will be created that will use the empty constructor.
        /// </summary>
        /// <typeparam name="TState"></typeparam>
        public void RegisterStateType<TState>() where TState : IPdaState<T>, new()
        {
            var type = typeof(TState);
            RegisterStateType(type, () => new TState());
        }

        /// <summary>
        /// Registers a state type in the machine. 
        /// 
        /// A pool for the type will be created which uses the given creationMethod to create new instance when needed.
        /// </summary>
        /// <param name="type">The state type to register.</param>
        /// <param name="creationMethod">The factory method to use for creating instances.</param>
        public void RegisterStateType(Type type, Func<IPdaState<T>> creationMethod)
        {
            var pool = new Pool<IPdaState<T>>(creationMethod);
            _statePools[type] = pool;
        }

        /// <summary>
        /// Checks whether a state type is registered in the machine.
        /// </summary>
        /// <typeparam name="TState">The state type to check for.</typeparam>
        /// <returns>True if registered, otherwise false.</returns>
        public bool IsRegistered<TState>()
        {
            return _statePools.ContainsKey(typeof(TState));
        }
    }
}
