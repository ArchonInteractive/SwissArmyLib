# ![Logo](https://gitlab.com/archoninteractive/SwissArmyLib/raw/master/logo.png) &nbsp; SwissArmyLib
## Please note this library is under construction, and the API is definitely not stable. Also some things might currently be untested and broken.

SwissArmyLib is an attempt to create a collection of useful utilities primarily intended for Unity projects, but feel free to rip parts out and use them for whatever you want.

A very important part in the design decisions of this library was to keep the garbage generation low. This means you will probably frown a little upon the use of interfaces for callbacks, instead of just using delicious delegates. It also means using the trusty old *for* loops for iterating through collections where possible.

There's a lot of libraries with some of the same features, but they're often walled off behind a restrictive or ambiguous license. 
This project is under the very permissive MIT license and we honestly do not care what you use it for.

### Features
* Events
    * Uses interfaces instead of delegates to reduce garbage
    * Can be prioritized to control call order
* Timers
    * Supports both scaled and unscaled time
    * Optional args to pass in
    * Also uses interfaces for callbacks to avoid garbage
* Automata
    * Finite State Machine
    * Pushdown Automaton
* Pooling
    * Support for both arbitrary classes and GameObjects
    * IPoolable interface for callbacks
    * Timed despawns
* Service Locator
	* A simple implementation of the Service Locator pattern
* Gravity
    * Flexible gravitational system
    * Useful for planet gravity, black holes, magnets and all that sort of stuff.
* Misc
    * Shake
        * Useful for creating proper screen shake
	* Lazy&lt;T&gt;
		* A backport of System.Lazy&lt;T&gt; from .NET 4.0+
    * Some niche collection types
	* A few other tiny utilities

### Download
There's currently no downloadable binary, but you can either build it yourself or just copy the files into your Unity project.

### License
MIT

### Contributing
Pull requests are very welcome!

I might deny new features if they're too niche though, but it's still very much appreciated!