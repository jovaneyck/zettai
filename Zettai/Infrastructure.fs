module Infrastructure

type Event<'tid, 'tdata> = { AggregateId: 'tid; Data: 'tdata }
type CommandHandler<'tid, 'tcmd, 'tevent> = 'tcmd -> Event<'tid, 'tevent> list
type EventStream<'tid, 'tevent> = 'tid -> 'tevent seq
type EventWriter<'tid, 'tevent> = Event<'tid, 'tevent> -> unit
