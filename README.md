# ![Logo](https://gitlab.com/archoninteractive/SwissArmyLib/raw/master/logo.png) &nbsp; SwissArmyLib
**Please note this library is under construction and the API is definitely not stable. Also some things might currently be untested and broken.**

---
[Download (bleeding edge)](https://archoninteractive.com/swissarmylib/downloads/Release.zip)
&#8226;
[Documentation](https://github.com/ArchonInteractive/SwissArmyLib/wiki)
&#8226;
[API Reference](https://archoninteractive.com/swissarmylib/)

---

### About
**SwissArmyLib** is an attempt to create a collection of useful utilities primarily intended for Unity projects, but feel free to rip parts out and use them for whatever you want.

A very important part in the design decisions of this library was to keep the garbage generation low. This means you will probably frown a little upon the use of interfaces for callbacks, instead of just using delicious delegates. It also means using the trusty old *for* loops for iterating through collections where possible.

There's a lot of libraries with some of the same features, but they're often walled off behind a restrictive or ambiguous license.
This project is under the very permissive MIT license and we honestly do not care what you use it for.

### Features
* [Events](https://github.com/ArchonInteractive/SwissArmyLib/wiki/Event)
    * Uses interfaces instead of delegates to reduce garbage
    * Can be prioritized to control call order
    * Check out [GlobalEvents](https://github.com/ArchonInteractive/SwissArmyLib/wiki/GlobalEvents) if you need.. well.. global events.
* [Timers](https://github.com/ArchonInteractive/SwissArmyLib/wiki/TellMeWhen)
    * Supports both scaled and unscaled time
    * Optional arbitrary args to pass in
    * Also uses interfaces for callbacks to avoid garbage
* Automata
    * [Finite State Machine](https://github.com/ArchonInteractive/SwissArmyLib/wiki/Finite-State-Machine)
    * [Pushdown Automaton](https://github.com/ArchonInteractive/SwissArmyLib/wiki/Pushdown-Automaton)
* [Pooling](https://github.com/ArchonInteractive/SwissArmyLib/wiki/Object-Pooling)
    * Support for both arbitrary classes and GameObjects
    * IPoolable interface for callbacks
        * PoolableGroup component in case multiple IPoolable components needs to be notified
    * Timed despawns
* [Service Locator](https://github.com/ArchonInteractive/SwissArmyLib/wiki/Service-Locator)
    * An implementation of the Service Locator pattern
    * Aware of MonoBehaviours and how to work with them
    * Supports scene-specific resolvers
    * Supports both singletons and short-lived objects
        * Singletons can be lazy loaded
* [Managed Update Loop](https://github.com/ArchonInteractive/SwissArmyLib/wiki/ManagedUpdate)
    * An update loop maintained in managed space to avoid the [overhead of Native C++ --> Managed C#](https://blogs.unity3d.com/2015/12/23/1k-update-calls/)
    * Useful for non-MonoBehaviours that needs to be part of the update loop
    * Optional [ManagedUpdateBehaviour](https://github.com/ArchonInteractive/SwissArmyLib/wiki/ManagedUpdateBehaviour) class for easy usage
* [Resource Pool](https://github.com/ArchonInteractive/SwissArmyLib/wiki/ResourcePool)
    * Generic and flexible resource pool (health, mana, energy etc.)
* [Gravity](https://github.com/ArchonInteractive/SwissArmyLib/wiki/GravitationalSystem)
    * Flexible gravitational system
    * Useful for planet gravity, black holes, magnets and all that sort of stuff.
* Misc
    * [Shake](https://github.com/ArchonInteractive/SwissArmyLib/wiki/Shake)
        * Useful for creating proper screen shake
    * [Some niche collection types](https://github.com/ArchonInteractive/SwissArmyLib/wiki/Collections)
    * [Some useful attributes](https://github.com/ArchonInteractive/SwissArmyLib/wiki/Attributes)
        * [ExecutionOrder](https://github.com/ArchonInteractive/SwissArmyLib/wiki/Attributes#executionorder)
            * Sets a default (or forces) an execution order for a MonoBehaviour
        * [ReadOnly](https://github.com/ArchonInteractive/SwissArmyLib/wiki/Attributes#readonly)
            * Makes fields uninteractable in the inspector
    * A few other tiny utilities

### Download
Binaries for the bleeding edge can be found [here](https://archoninteractive.com/swissarmylib/downloads/Release.zip).
Alternatively you can either [build it yourself](https://github.com/ArchonInteractive/SwissArmyLib/wiki/Home#building-the-source) (very easily) or simply [copy the source code into your Unity project](https://github.com/ArchonInteractive/SwissArmyLib/wiki/Home#method-2-copy-source) and call it a day.

### License
MIT

### Contributing
Pull requests are very welcome!

I might deny new features if they're too niche though, but it's still very much appreciated!
