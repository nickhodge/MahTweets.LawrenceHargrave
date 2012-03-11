var ts = Observable.FromEvent<EventArgs>(txt, "TextChanged);
var res = from e in ts
select ((TextBox)e.Sender).Text;
using (res.Subscribe(Console.WriteLine);


- Scheduler.ThreadPool (CoreEx)
- ObserveOn does thread sync
- look at Reactive operators such as DistinctUntilChanged()
- and throttle(Timespan())