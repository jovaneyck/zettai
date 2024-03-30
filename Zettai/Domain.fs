module Domain

open Infrastructure

exception ValidationException of string

type User = User of string

type ListId = ListId of System.Guid

module ListId =
    let mint () = System.Guid.NewGuid() |> ListId

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
    { Id: ListId
      Name: ListName
      Items: ToDoItem list }

[<AutoOpen>]
module ToDoList =
    let initial id name = { Id = id; Name = name; Items = [] }

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

type ListCreatedData = { User: User; ListName: ListName }
type ItemAddedToListData = { Item: ItemData }

type ListEvent =
    | ListCreated of ListCreatedData
    | ItemAddedToList of ItemAddedToListData

module ListEvent =
    let fold acc events =
        let folder list event =
            match event.Data with
            | ListCreated c ->
                { Id = event.AggregateId
                  Name = c.ListName
                  Items = [] }
            | ItemAddedToList a -> { list with Items = { Description = a.Item.Description } :: list.Items }

        events |> Seq.fold folder acc

let createList (cmd: CreateListData) =
    let id = ListId.mint ()

    [ { AggregateId = id
        Data = ListCreated { User = cmd.User; ListName = cmd.List } } ]

let addItemToList (lookup: ListLookup) (cmd: AddItemToListData) =
    let list = lookup cmd.User cmd.List

    [ { AggregateId = list.Id
        Data = ItemAddedToList { Item = { Description = cmd.Item.Description } } } ]

let handleCommand (lookup: ListLookup) : CommandHandler<ListId, ListCommand, ListEvent> =
    fun cmd ->
        match cmd with
        | CreateList c -> createList c
        | AddItemToList d -> addItemToList lookup d
