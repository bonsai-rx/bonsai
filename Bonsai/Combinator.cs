﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Bonsai
{
    [Combinator]
    [XmlType("CombinatorBase")]
    public abstract class Combinator
    {
        public abstract IObservable<TSource> Process<TSource>(IObservable<TSource> source);
    }

    [Combinator]
    public abstract class Combinator<TResult>
    {
        public abstract IObservable<TResult> Process<TSource>(IObservable<TSource> source);
    }

    [Combinator]
    public abstract class Combinator<TSource, TResult>
    {
        public abstract IObservable<TResult> Process(IObservable<TSource> source);
    }
}
