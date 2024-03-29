module Types

exception ValidationException of string

type User = User of string
type ListName = private ListName of string

module ListName =
    let fromTrusted (text: string) = ListName text

    let fromUntrusted (text: string) =
        if text.Length > 40 then
            ValidationException("should not be more than 40 chars")
            |> raise
        else
            ListName text

type ToDoItem = { Description: string }

type ToDoList =
    { Name: ListName
      Items: ToDoItem list }

type ListLookup = User -> ListName -> ToDoList
type ListWrite = User -> ToDoList -> unit
