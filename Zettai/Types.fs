module Types

type Event<'tid, 'tdata> = { AggregateId: 'tid; Data: 'tdata }
type CommandHandler<'tid, 'tcmd, 'tevent> = 'tcmd -> Event<'tid, 'tevent> list
type EventStream<'tid, 'tevent> = 'tid -> 'tevent seq
type EventWriter<'tid, 'tevent> = Event<'tid, 'tevent> -> unit

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

[<AutoOpen>]
module ToDoList =
    let initial name = { Name = name; Items = [] }

type ListLookup = User -> ListName -> ToDoList

type CreateListData = { User: User; List: ListName }
type ItemData = { Description: string }

type AddItemToListData =
    { User: User
      List: ListName
      Item: ItemData }

type ListCommand =
    | CreateList of CreateListData
    | AddItemToList of AddItemToListData

type ListCreatedData = { User: User; List: ListName }
type ItemAddedToListData = { List: ListName; Item: ItemData }

type ListEvent =
    | ListCreated of ListCreatedData
    | ItemAddedToList of ItemAddedToListData

module ListEvent =
    let fold acc events =
        let folder list event =
            match event with
            | ListCreated c -> { Name = c.List; Items = [] }
            | ItemAddedToList a -> { list with Items = { Description = a.Item.Description } :: list.Items }

        events |> Seq.fold folder acc

let createList (cmd: CreateListData) =
    [ { AggregateId = cmd.List
        Data = ListCreated { User = cmd.User; List = cmd.List } } ]

let addItemToList (cmd: AddItemToListData) =
    [ { AggregateId = cmd.List
        Data =
          ItemAddedToList
              { List = cmd.List
                Item = { Description = cmd.Item.Description } } } ]

let handleCommand (lookup: ListLookup) : CommandHandler<ListName, ListCommand, ListEvent> =
    fun cmd ->
        match cmd with
        | CreateList c -> createList c
        | AddItemToList d -> addItemToList d
