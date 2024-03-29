module Types

type User = User of string
type ListName = ListName of string
type ToDoItem = { Description: string }

type ToDoList =
    { Name: ListName
      Items: ToDoItem list }

type ListLookup = User -> ListName -> ToDoList
