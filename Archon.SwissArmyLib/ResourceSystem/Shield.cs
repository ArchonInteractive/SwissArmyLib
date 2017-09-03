using Archon.SwissArmyLib.Events;
using Archon.SwissArmyLib.Utils.Editor;
using JetBrains.Annotations;
using UnityEngine;

namespace Archon.SwissArmyLib.ResourceSystem
{
    public class Shield : ResourcePool, IEventListener<IResourcePreChangeEvent>, IEventListener<IResourceEvent>
    {
        [SerializeField, ReadOnly(OnlyWhilePlaying = true)]
        private ResourcePool _protectedTarget;

        [SerializeField]
        private float _absorptionFlat;

        [SerializeField, Range(0, 1)]
        private float _absorptionScaling = 0.5f;

        [SerializeField]
        private bool _emptiesWithTarget = true;

        [SerializeField]
        private bool _renewsWithTarget = true;

        public ResourcePool ProtectedTarget
        {
            get { return _protectedTarget; }
            set
            {
                if (_protectedTarget != null && enabled)
                {
                    _protectedTarget.OnPreChange.RemoveListener(this);
                    _protectedTarget.OnEmpty.RemoveListener(this);
                }

                _protectedTarget = value;

                if (_protectedTarget != null && enabled)
                {
                    _protectedTarget.OnPreChange.AddListener(this);
                    _protectedTarget.OnEmpty.AddListener(this);
                }
            }
        }

        public float AbsorptionFlat
        {
            get { return _absorptionFlat; }
            set { _absorptionFlat = value; }
        }

        public float AbsorptionScaling
        {
            get { return _absorptionScaling; }
            set { _absorptionScaling = value; }
        }

        public bool EmptiesWithTarget
        {
            get { return _emptiesWithTarget; }
            set { _emptiesWithTarget = value; }
        }

        public bool RenewsWithTarget
        {
            get { return _renewsWithTarget; }
            set { _renewsWithTarget = value; }
        }

        /// <inheritdoc />
        [UsedImplicitly]
        protected override void Awake()
        {
            base.Awake();

            if (_protectedTarget == null)
                _protectedTarget = GetComponent<ResourcePool>();
        }

        /// <summary>
        /// Called when the MonoBehaviour is enabled.
        /// </summary>
        [UsedImplicitly]
        protected void OnEnable()
        {
            if (_protectedTarget != null)
            {
                _protectedTarget.OnPreChange.AddListener(this);
                _protectedTarget.OnEmpty.AddListener(this);
                _protectedTarget.OnRenew.AddListener(this);
            }
        }

        /// <summary>
        /// Called when the MonoBehaviour is disabled.
        /// </summary>
        [UsedImplicitly]
        protected void OnDisable()
        {
            if (_protectedTarget != null)
            {
                _protectedTarget.OnPreChange.RemoveListener(this);
                _protectedTarget.OnEmpty.RemoveListener(this);
                _protectedTarget.OnRenew.RemoveListener(this);
            }
        }

        /// <inheritdoc />
        protected override float Change(float delta, object source = null, object args = null, bool forced = false)
        {
            if (_emptiesWithTarget && ProtectedTarget.IsEmpty && !forced)
                return 0;

            return base.Change(delta, source, args, forced);
        }

        /// <inheritdoc />
        public void OnEvent(int eventId, IResourcePreChangeEvent args)
        {
            if (args.ModifiedDelta < 0 && !IsEmpty)
            {
                var absorbed = AbsorptionFlat;
                absorbed += -args.ModifiedDelta * AbsorptionScaling;
                absorbed = Mathf.Clamp(absorbed, 0, Mathf.Min(-args.ModifiedDelta, Current));

                args.ModifiedDelta += absorbed;

                Remove(absorbed, args.Source, args.Args);
            }
        }

        /// <inheritdoc />
        public void OnEvent(int eventId, IResourceEvent args)
        {
            switch (eventId)
            {
                case EventIds.Empty:
                    if (_emptiesWithTarget)
                        Empty(args.Source, args.Args, true);
                    return;
                case EventIds.Renew:
                    if (_renewsWithTarget)
                        Renew(args.Source, args.Args, true);
                    return;
            }
        }
    }
}
