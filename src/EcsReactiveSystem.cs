// ----------------------------------------------------------------------------
// The MIT License
// Reactive behaviour for Entity Component System framework https://github.com/Leopotam/ecs
// Copyright (c) 2017-2019 Leopotam <leopotam@gmail.com>
// ----------------------------------------------------------------------------

using System;
using System.Runtime.CompilerServices;

namespace Leopotam.Ecs.Reactive {
    /// <summary>
    /// Type of reaction.
    /// </summary>
    public enum EcsReactiveType {
        OnAdded,
        OnRemoved
    }

    /// <summary>
    /// Base class for all reactive systems.
    /// </summary>
    public abstract class EcsReactiveSystemBase : IEcsFilterListener, IEcsPreInitSystem, IEcsAfterDestroySystem, IEcsRunSystem {
        EcsEntity[] _reactedEntities = new EcsEntity[32];
        int _reactedEntitiesCount;
        EcsReactiveType _reactType;

        void IEcsPreInitSystem.PreInit () {
            _reactType = GetReactiveType ();
            GetFilter ().AddListener (this);
        }

        void IEcsAfterDestroySystem.AfterDestroy () {
            _reactedEntitiesCount = 0;
            GetFilter ().RemoveListener (this);
        }

        void IEcsRunSystem.Run () {
            if (_reactedEntitiesCount > 0) {
                RunReactive ();
                _reactedEntitiesCount = 0;
            }
        }

        void IEcsFilterListener.OnEntityAdded (in EcsEntity entity) {
            if (_reactType == EcsReactiveType.OnAdded) {
                if (_reactedEntities.Length == _reactedEntitiesCount) {
                    Array.Resize (ref _reactedEntities, _reactedEntitiesCount << 1);
                }
                _reactedEntities[_reactedEntitiesCount++] = entity;
            }
        }

        void IEcsFilterListener.OnEntityRemoved (in EcsEntity entity) {
            if (_reactType == EcsReactiveType.OnRemoved) {
                if (_reactedEntities.Length == _reactedEntitiesCount) {
                    Array.Resize (ref _reactedEntities, _reactedEntitiesCount << 1);
                }
                _reactedEntities[_reactedEntitiesCount++] = entity;
            }
        }

        /// <summary>
        /// Returns EcsFilterReactive instance for watching on it.
        /// </summary>
        protected abstract EcsFilter GetFilter ();

        /// <summary>
        /// Returns reactive type behaviour.
        /// </summary>
        protected abstract EcsReactiveType GetReactiveType ();

        /// <summary>
        /// Processes reacted entities.
        /// Will be called only if any entities presents for processing.
        /// </summary>
        protected abstract void RunReactive ();

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public Enumerator GetEnumerator () {
            return new Enumerator (_reactedEntities, _reactedEntitiesCount);
        }

        public struct Enumerator {
            readonly EcsEntity[] _entities;
            readonly int _count;
            int _idx;

            [MethodImpl (MethodImplOptions.AggressiveInlining)]
            internal Enumerator (EcsEntity[] entities, int entitiesCount) {
                _entities = entities;
                _count = entitiesCount;
                _idx = -1;
            }

            public ref EcsEntity Current {
                [MethodImpl (MethodImplOptions.AggressiveInlining)]
                get { return ref _entities[_idx]; }
            }

            [MethodImpl (MethodImplOptions.AggressiveInlining)]
            public void Dispose () { }

            [MethodImpl (MethodImplOptions.AggressiveInlining)]
            public bool MoveNext () {
                return ++_idx < _count;
            }
        }
    }

    /// <summary>
    /// Reactive system with support for custom filter.
    /// </summary>
    /// <typeparam name="F">First component type.</typeparam>
    public abstract class EcsReactiveSystem<F> : EcsReactiveSystemBase where F : EcsFilter {
        protected readonly F ReactiveFilter = null;

        protected sealed override EcsFilter GetFilter () {
            return ReactiveFilter;
        }
    }

    /// <summary>
    /// For internal use only! Special component for mark user components as updated.
    /// </summary>
    /// <typeparam name="T">User component type.</typeparam>
    // ReSharper disable once ClassNeverInstantiated.Global
    // ReSharper disable once UnusedTypeParameter
    public class EcsUpdateReactiveFlag<T> : IEcsIgnoreInFilter where T : class { }

    /// <summary>
    /// Reactive system for processing updated components (EcsWorld.MarkComponentAsUpdated).
    /// </summary>
    /// <typeparam name="Inc1">Component type.</typeparam>
    public abstract class EcsUpdateReactiveSystem<Inc1> : EcsReactiveSystemBase where Inc1 : class {
        /// <summary>
        /// Internal filter for custom reaction on entities.
        /// </summary>
        protected readonly EcsFilter<EcsUpdateReactiveFlag<Inc1>> ReactiveFilter = null;

        protected sealed override EcsFilter GetFilter () {
            return ReactiveFilter;
        }

        protected sealed override EcsReactiveType GetReactiveType () {
            return EcsReactiveType.OnAdded;
        }

        protected sealed override void RunReactive () {
            foreach (var idx in ReactiveFilter) {
                ReactiveFilter.Entities[idx].Unset<EcsUpdateReactiveFlag<Inc1>> ();
            }

            RunUpdateReactive ();
        }

        /// <summary>
        /// Processes MarkComponentAsUpdated reacted entities.
        /// Will be called only if any entities presents for processing.
        /// </summary>
        protected abstract void RunUpdateReactive ();
    }
}