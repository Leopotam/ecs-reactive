// ----------------------------------------------------------------------------
// The MIT License
// Reactive behaviour for Entity Component System framework https://github.com/Leopotam/ecs
// Copyright (c) 2017-2019 Leopotam <leopotam@gmail.com>
// ----------------------------------------------------------------------------

// ReSharper disable once CheckNamespace
namespace Leopotam.Ecs {
    public static class EcsWorldExtensions {
        /// <summary>
        /// Marks component on entity as updated for processing later at reactive system.
        /// </summary>
        /// <typeparam name="T">Component type.</typeparam>
        public static void MarkAsUpdated<T> (this EcsEntity entity) where T : class, new () {
            entity.Set<Reactive.EcsUpdateReactiveFlag<T>> ();
        }
    }
}