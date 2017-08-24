using System;
using System.Collections.Generic;
using Archon.SwissArmyLib.Pooling;

namespace Archon.SwissArmyLib.Automata
{
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
                    currentState.Think(deltaTime);
            }
        }

        /// <summary>
        /// Replaces the active state with another state, without notifying the underlying state.
        /// </summary>
        /// <typeparam name="TState"></typeparam>
        /// <returns></returns>
        public TState ChangeState<TState>() where TState : IPdaState<T>
        {
            PopStateSilently();
            return PushStateSilently<TState>();
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
        /// <typeparam name="TState"></typeparam>
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
        /// <typeparam name="TState"></typeparam>
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
        /// Obtains a pooled instance of the given type.
        /// </summary>
        /// <typeparam name="TState"></typeparam>
        /// <returns></returns>
        private TState ObtainState<TState>() where TState : IPdaState<T>
        {
            var pool = GetPool(typeof(TState));
            return (TState) pool.Spawn();
        }

        /// <summary>
        /// Frees a state instance and makes it available for reuse.
        /// </summary>
        /// <param name="state"></param>
        private void FreeState(IPdaState<T> state)
        {
            var type = state.GetType();
            var pool = GetPool(type);
            pool.Despawn(state);
        }

        /// <summary>
        /// Gets a pool for the given state type.
        /// </summary>
        /// <param name="stateType"></param>
        /// <returns></returns>
        private Pool<IPdaState<T>> GetPool(Type stateType)
        {
            Pool<IPdaState<T>> pool;
            _statePools.TryGetValue(stateType, out pool);
            return pool;
        }

        /// <summary>
        /// Registers a state type in the machine. 
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
        /// A pool for the type will be created which uses the given creationMethod to create new instance when needed.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="creationMethod"></param>
        public void RegisterStateType(Type type, Func<IPdaState<T>> creationMethod)
        {
            var pool = new Pool<IPdaState<T>>(creationMethod);
            _statePools[type] = pool;
        }
    }
}
