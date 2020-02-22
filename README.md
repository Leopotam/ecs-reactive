[![gitter](https://img.shields.io/gitter/room/leopotam/ecs.svg)](https://gitter.im/leopotam/ecs)
[![license](https://img.shields.io/github/license/Leopotam/ecs-reactive.svg)](https://github.com/Leopotam/ecs-reactive/blob/develop/LICENSE)
# Reactive behaviour for LeoECS
Reactive filters / systems for using with [Entity Component System Framework](https://github.com/Leopotam/ecs).

> C#7.3 or above required for this framework.

> Tested on unity 2019.1 (not dependent on it) and contains assembly definition for compiling to separate assembly file for performance reason.

> **Important!** All reacted entities are just cached results: real entities / components already can be removed from world / component! If you know that you use similar behaviour (entity can be removed before reactive system starts to work) - `EcsWorld.IsEntityExists` method should be used at reactive system processing per each entity. But better to not remove entities before reactive systems.

# Installation

## As unity module
This repository can be installed as unity module directly from git url. In this way new line should be added to `Packages/manifest.json`:
```
"com.leopotam.ecs-reactive": "https://github.com/Leopotam/ecs-reactive.git",
```
By default last released version will be used. If you need trunk / developing version then `develop` name of branch should be added after hash:
```
"com.leopotam.ecs-reactive": "https://github.com/Leopotam/ecs-reactive.git#develop",
```

## As source
If you can't / don't want to use unity modules, code can be downloaded as sources archive of required release from [Releases page](`https://github.com/Leopotam/ecs-reactive/releases`).


# Examples

## OnAdd / OnRemove example:
```csharp
class ReactiveComponent1 {
    public int Id;
}

sealed class TestReactiveStartup : MonoBehaviour {
    EcsWorld _world;
    EcsSystems _systems;

    void OnEnable () {
        _world = new EcsWorld ();
        _systems = new EcsSystems (_world);
        _systems
            .Add (new TestReactiveSystemOnAdd ())
            .Add (new TestReactiveSystemOnRemove ())
            .Init ();
    }

    void Update () {
        _systems.Run ();
    }

    void OnDisable () {
        _systems.Destroy ();
        _systems = null;
        _world.Destroy ();
        _world = null;
    }
}

sealed class TestReactiveSystemOnAdd : EcsReactiveSystem<EcsFilter<ReactiveComponent1>>, IEcsInitSystem {
    EcsWorld _world = null;

    void IEcsInitSystem.Init () {
        // create test data for catching OnAdded event at react system later.
        _world.NewEntityWith<ReactiveComponent1> (out var c1);
        c1.Id = 10;
        _world.NewEntityWith<ReactiveComponent1> (out var c2);
        c2.Id = 20;
        _world.NewEntityWith<ReactiveComponent1> (out var c3);
        c3.Id = 30;
    }

    protected override EcsReactiveType GetReactiveType () {
        // this system should react on Add event.
        return EcsReactiveType.OnAdded;
    }

    // this method will be called only if there are any entities for processing.
    protected override void RunReactive () {
        // Proper way to iterate over filtered entities collection.
        foreach (ref var entity in this) {
            var c = entity.Get<ReactiveComponent1> ();
            Debug.LogFormat ("[ON-ADDED] Reacted entity \"{0}\" and component {1}", entity, c.Id);

            // remove reacted entities for test OnRemoved reactive system.
            entity.Destroy();
        }
    }
}

sealed class TestReactiveSystemOnRemove : EcsReactiveSystem<EcsFilter<ReactiveComponent1>> {
    protected override EcsReactiveType GetReactiveType () {
        // this system should react on Remove event.
        return EcsReactiveType.OnRemoved;
    }

    // this method will be called only if there are any entities for processing.
    protected override void RunReactive () {
        foreach (ref var entity in this) {
            Debug.LogFormat ("[ON-REMOVE] Reacted entity: {0}", entity);
        }
    }
}
```

Result log:
```
[ON-ADDED] Reacted entity "0" and component 10
[ON-ADDED] Reacted entity "1" and component 20
[ON-ADDED] Reacted entity "2" and component 30
[ON-REMOVE] Reacted entity: 0
[ON-REMOVE] Reacted entity: 1
[ON-REMOVE] Reacted entity: 2
```

## MarkComponentAsUpdated example:
```csharp
class UpdateComponent1 {
    public int Id;
}

sealed class TestUpdateReactiveStartup : MonoBehaviour {
    EcsWorld _world;
    EcsSystems _systems;

    void OnEnable () {
        _world = new EcsWorld ();
        _systems = new EcsSystems (_world);
        _systems
            .Add (new TestRunSystem ())
            .Add (new TestReactiveSystemOnUpdate ())
            .Init ();
    }

    void Update () {
        _systems.Run ();
    }

    void OnDisable () {
        _systems.Destroy ();
        _systems = null;
        _world.Destroy ();
        _world = null;
    }
}

[EcsInject]
sealed class TestRunSystem : IEcsInitSystem, IEcsRunSystem {
    EcsWorld _world = null;
    EcsFilter<UpdateComponent1> _filter = null;

    void IEcsInitSystem.Init () {
        _world.CreateEntityWith<UpdateComponent1> ().Id = 10;
    }

    void IEcsRunSystem.Run () {
        foreach (var idx in _filter) {
            _filter.Components1[idx].Id++;
            // Important! This method should be called for each component for processing at EcsUpdateReactiveSystem.
            _filter.Entities[idx].MarkAsUpdated<UpdateComponent1> ();
        }
    }
}

[EcsInject]
sealed class TestReactiveSystemOnUpdate : EcsUpdateReactiveSystem<EcsFilter<UpdateComponent1>> {
    // this method will be called only if there are any entities for processing.
    protected override void RunUpdateReactive () {
        foreach (ref var entity in this) {
            var c = entity.Get<UpdateComponent1> ();
            Debug.LogFormat ("[ON-UPDATE] Updated entity: {0}, new value: {1}", entity, c.Id);
        }
    }
}
```

Result log:
```
[ON-UPDATE] Updated entity: 0, new value: 11
[ON-UPDATE] Updated entity: 0, new value: 12
[ON-UPDATE] Updated entity: 0, new value: 13
[ON-UPDATE] Updated entity: 0, new value: 14
...
```

# License
The software released under the terms of the [MIT license](./LICENSE.md). Enjoy.

# Donate
Its free opensource software, but you can buy me a coffee:

<a href="https://www.buymeacoffee.com/leopotam" target="_blank"><img src="https://www.buymeacoffee.com/assets/img/custom_images/yellow_img.png" alt="Buy Me A Coffee" style="height: auto !important;width: auto !important;" ></a>