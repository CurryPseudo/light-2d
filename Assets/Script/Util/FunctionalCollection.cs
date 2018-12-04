using System;
using System.Collections;
using System.Collections.Generic;
public class FunctionalCollection<T> : IEnumerable<T>{
    private IEnumerable<T> ts;
    public FunctionalCollection(IEnumerable<T> ts) {
        this.ts = ts;
    }

    public IEnumerator<T> GetEnumerator()
    {
        return ts.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ts.GetEnumerator();
    }
    public FunctionalCollection<G> Map<G>(Func<T, G> from) {
        return new FunctionalCollection<G>(MapInternal(from));
    }
    private IEnumerable<G> MapInternal<G>(Func<T, G> from) {
        foreach(var t in ts) {
            yield return from(t);
        }
    }
    public FunctionalCollection<T> Filter(Func<T, bool> valid) {
        return new FunctionalCollection<T>(FilterInternal(valid));
    }
    private IEnumerable<T> FilterInternal(Func<T, bool> valid) {
        foreach(var t in ts) {
            if(valid(t)) {
                yield return t;
            }
        }
    }
    public G Foldl<G>(G origin, Func<G, T, G> step) {
        foreach(var t in ts) {
            origin = step(origin, t);
        }
        return origin;
    }
    public int Length {
        get {
            return Foldl(0, (l, t) => l + 1);
        }
    }

}