using System;
using System.Collections.Generic;
using System.Reactive.Linq;

namespace Lykke.Service.BitstampAdapter.Services
{
    public static class ObservableExtensions
    {
        private static IEnumerable<IObservable<T>> Generator<T>(IObservable<T> source, TimeSpan min, TimeSpan max)
        {
            TimeSpan? delay = null;

            while (true)
            {
                var delayed =  delay == null? source : source.DelaySubscription(delay.Value);

                var result = delayed.Do(_ => { delay = null; }, err =>
                {
                    if (delay == null) delay = min;
                    else
                    {
                        var nextMs = delay.Value.Milliseconds * 2;
                        delay = TimeSpan.FromMilliseconds(Math.Min(nextMs, max.TotalMilliseconds));
                    }
                });

                yield return result
                    .Select(x => (true, x))
                    .Catch((Exception ex) => Observable.Return((false, default(T))))
                    .Where(x => x.Item1)
                    .Select(x => x.Item2);
            }
            // ReSharper disable once IteratorNeverReturns
        }

        public static IObservable<T> RetryWithBackoff<T>(
            this IObservable<T> source,
            TimeSpan min,
            TimeSpan max)
        {
            return Generator(source, min, max).Concat();
        }

        public static IObservable<long> WindowCount<T>(this IObservable<T> source, TimeSpan window)
        {
            var runningTotal = source.Scan((long) 0, (c, _) => c + 1).StartWith(0);

            var totalDelayed = runningTotal.Delay(window).StartWith(0);

            return Observable.CombineLatest(runningTotal, totalDelayed, (t, d) => t - d);
        }
    }
}
