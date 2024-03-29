# Building a fully functional web API

Based on the contents of [From objects to functions](https://pragprog.com/titles/uboop/from-objects-to-functions/) by Uberto Barbini.

* Series introduction: what will we build and how will we build it?
    * Zettai, a TODO list manager for multiple users
    * web API
    * In a functional language, with functional libraries and idioms
    * Persistence
    * No: security, CICD
* Hello world! Building a walking skeleton (functional web server, acceptance tests)
* Our first story: modify a list with outside-in TDD. Modeling the domain with types.
* Event sourcing: commands change state, events to represent changes
* Creating a new list. 
* Better error handling using Results. Hello fmap and functor!
* Representing the current state: projections
* Adding persistence: reader monad
* Combining error handling and database access: monad transformers
* Input validation: applicatives

# Add-ons

* Persisting projections
* Integrating with dotnet libraries: logging and JSON serde