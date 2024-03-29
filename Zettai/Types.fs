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

type CreateListData = { User: User; List: ListName }
type ItemData = { Description: string }

type AddItemToListData =
    { User: User
      List: ListName
      Item: ItemData }

type ListCommand =
    | CreateList of CreateListData
    | AddItemToList of AddItemToListData

type ListEvent =
    | ListCreated of CreateListData
    | ItemAddedToList of AddItemToListData

type CommandHandler = ListCommand -> ListEvent list
type EventWriter = ListEvent -> unit

let createList writer (cmd: CreateListData) =
    let list = { Name = cmd.List; Items = [] }
    writer cmd.User list
    [ ListCreated cmd ]

let addItemToList lookup writer (cmd: AddItemToListData) =
    let list = lookup cmd.User cmd.List
    let newItem: ToDoItem = { Description = cmd.Item.Description }
    let updated = { list with Items = newItem :: list.Items }
    writer cmd.User updated
    [ ItemAddedToList cmd ]

let handleCommand (lookup: ListLookup) (writer: ListWrite) (cmd: ListCommand) : ListEvent list =
    match cmd with
    | CreateList c -> createList writer c
    | AddItemToList d -> addItemToList lookup writer d
