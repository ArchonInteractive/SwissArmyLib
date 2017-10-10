.. image:: https://raw.githubusercontent.com/ArchonInteractive/SwissArmyLib/master/logo.png

SwissArmyLib
============

.. image:: https://ci.appveyor.com/api/projects/status/sapkbwkbl5ug901u/branch/master?svg=true
    :alt: Build status
    :target: https://ci.appveyor.com/project/Phault/swissarmylib/branch/master

.. image:: https://readthedocs.org/projects/swissarmylib-docs/badge/?version=latest
    :alt: Documentation Status
    :target: http://swissarmylib-docs.readthedocs.io/en/latest/?badge=latest

.. image:: https://api.bintray.com/packages/phault/SwissArmyLib/development/images/download.svg
    :alt: Download
    :target: https://bintray.com/phault/SwissArmyLib/development/_latestVersion#files

`API Reference <https://archoninteractive.com/swissarmylib/>`_

----

About
-----

**SwissArmyLib** is an attempt to create a collection of useful utilities with a focus on being performant. It is primarily intended for Unity projects, but feel free to rip parts out and use them for whatever you want.

A very important part in the design decisions of this library was to keep the garbage generation low. This means you will probably frown a little upon the use of interfaces for callbacks, instead of just using delicious delegates. It also means using the trusty old *for* loops for iterating through collections where possible.

There's a lot of libraries with some of the same features, but they're often walled off behind a restrictive or ambiguous license.
This project is under the very permissive MIT license and we honestly do not care what you use it for.

Features
--------

*   Events_

    -   Supports both interface and delegate listeners
    -   Can be prioritized to control call order
    -   Check out GlobalEvents_ if you need.. well.. global events.
      
*   Timers_

    -   Supports both scaled and unscaled time
    -   Optional arbitrary args to pass in
    -   Also uses interfaces for callbacks to avoid garbage
    
*   Coroutines_

    -   More performant alternative to Unity's coroutines with a very similar API.
    
*   Automata_

    -   `Finite State Machine`_
    -   `Pushdown Automaton`_

*   Pooling_

    -   Support for both arbitrary classes and GameObjects
    -   IPoolable_ interface for callbacks

        +   PoolableGroup_ component in case multiple IPoolable components needs to be notified

    -   Timed despawns

*   `Service Locator`_

    -   An implementation of the Service Locator pattern
    -   Aware of MonoBehaviours and how to work with them
    -   Supports scene-specific resolvers
    -   Supports both singletons and short-lived objects

        +   Singletons can be lazy loaded

*   `Managed Update Loop`_

    -   An update loop maintained in managed space to avoid the `overhead of Native C++ --> Managed C# <https://blogs.unity3d.com/2015/12/23/1k-update-calls/>`_
    -   Useful for non-MonoBehaviours that needs to be part of the update loop
    -   Optional ManagedUpdateBehaviour_ class for easy usage

*   `Spatial Partitioning`_

    -   GC-friendly implementations of common space-partitioning systems

*   `Resource Pool`_

    -   Generic and flexible resource pool (health, mana, energy etc.)

*   Gravity

    -   Flexible gravitational system
    -   Useful for planet gravity, black holes, magnets and all that sort of stuff.

*   Misc

    -   BetterTime_

        +   A wrapper for Unity's static Time class that caches the values per frame to avoid the marshal overhead.
        +   About 4x faster than using the Time class directly, but we're talking miniscule differences here.

    -   Shake

        +   Useful for creating proper screen shake

    -   `Some collection types`_
    -   `Some useful attributes`_
        
        +   ExecutionOrder_

            *   Sets a default (or forces) an execution order for a MonoBehaviour

        +   ReadOnly_

            *   Makes fields uninteractable in the inspector

    -   A few other tiny utilities

Download
~~~~~~~~
Binaries for the bleeding edge can be found `here <download_>`_.
Alternatively you can either `build it yourself <building_>`_ (very easily) or simply `copy the source code into your Unity project <copysource_>`_ and call it a day.

License
~~~~~~~
`MIT <https://tldrlegal.com/license/mit-license>`_ - Do whatever you want. :) 

Contributing
~~~~~~~~~~~~
Pull requests are very welcome!

I might deny new features if they're too niche though, but it's still very much appreciated!

If you're looking for a way to contribute, please consider helping with the documentation at `this repository <https://github.com/ArchonInteractive/SwissArmyLib-docs>`_.

.. _download: https://bintray.com/phault/SwissArmyLib/development/_latestVersion#files
.. _building: https://swissarmylib-docs.readthedocs.io/en/latest/Getting%20Started.html#building-the-source
.. _copysource: https://swissarmylib-docs.readthedocs.io/en/latest/Getting%20Started.html#method-2-copy-source

.. _Events: https://swissarmylib-docs.readthedocs.io/en/latest/Events/Event.html
.. _GlobalEvents: https://swissarmylib-docs.readthedocs.io/en/latest/Events/GlobalEvents.html
.. _Timers: https://swissarmylib-docs.readthedocs.io/en/latest/Events/TellMeWhen.html
.. _Coroutines: https://swissarmylib-docs.readthedocs.io/en/latest/Coroutines/BetterCoroutines.html
.. _Automata: https://swissarmylib-docs.readthedocs.io/en/latest/Automata/index.html
.. _Finite State Machine: https://swissarmylib-docs.readthedocs.io/en/latest/Automata/Finite%20State%20Machine.html
.. _Pushdown Automaton: https://swissarmylib-docs.readthedocs.io/en/latest/Automata/Pushdown%20Automaton.html
.. _Pooling: https://swissarmylib-docs.readthedocs.io/en/latest/Pooling/index.html
.. _IPoolable: https://swissarmylib-docs.readthedocs.io/en/latest/Pooling/IPoolable.html
.. _PoolableGroup: https://swissarmylib-docs.readthedocs.io/en/latest/Pooling/PoolableGroup.html
.. _Service Locator: https://swissarmylib-docs.readthedocs.io/en/latest/Utils/Service%20Locator.html
.. _Managed Update Loop: https://swissarmylib-docs.readthedocs.io/en/latest/Events/ManagedUpdate.html
.. _ManagedUpdateBehaviour: https://swissarmylib-docs.readthedocs.io/en/latest/Events/ManagedUpdateBehaviour.html
.. _Spatial Partitioning: https://swissarmylib-docs.readthedocs.io/en/latest/Partitioning/index.html
.. _Resource Pool: https://swissarmylib-docs.readthedocs.io/en/latest/Resource%20System/index.html
.. _BetterTime: https://swissarmylib-docs.readthedocs.io/en/latest/Utils/BetterTime.html
.. _Some collection types: https://swissarmylib-docs.readthedocs.io/en/latest/Collections/index.html
.. _Some useful attributes: https://swissarmylib-docs.readthedocs.io/en/latest/Utils/Attributes/index.html
.. _ExecutionOrder: https://swissarmylib-docs.readthedocs.io/en/latest/Utils/Attributes/ExecutionOrder.html
.. _ReadOnly: https://swissarmylib-docs.readthedocs.io/en/latest/Utils/Attributes/ReadOnly.html