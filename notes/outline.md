# Building a fully functional F# web API

Based on the contents of [From objects to functions](https://pragprog.com/titles/uboop/from-objects-to-functions/) by Uberto Barbini.

1. Series introduction: what will we build and how will we build it?
    * Zettai, a TODO list manager for multiple users
    * We'll build the web API
    * In a functional language, with functional libraries and idioms
    * Using outside-in TDD
    * We will be incorporating persistence, but will leave other functional aspects as an exercise
2. Hello world! Building a walking skeleton (functional web server, acceptance tests)
3. Our first story: showing lists for a user
3. Our second and third story: adding items to an existing list, adding a new list 
 * Modeling the domain with types: make illegal state unrepresentable: user and listname invariants
4. Event sourcing: commands change state, events to represent changes
5. Creating a new list. 
6. Better error handling using Results. Hello fmap and functor!
7. Representing the current state: projections
8. Adding persistence: reader monad
9. Combining error handling and database access: monad transformers
10. Input validation: applicatives

## Add-ons

* Persisting projections
* Integrating with "non-functional" dotnet libraries: security, logging and JSON serde