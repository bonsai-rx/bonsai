using Bonsai.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Bonsai.Editor
{
    class WorkflowRuntimeExceptionCache
    {
        const int CollectionCheck = 10;
        readonly Dictionary<WeakKey<Exception>, Queue<ExpressionBuilder>> exceptions;
        int nextCollectionCheck;

        internal WorkflowRuntimeExceptionCache()
        {
            exceptions = new Dictionary<WeakKey<Exception>, Queue<ExpressionBuilder>>();
            nextCollectionCheck = CollectionCheck;
        }

        public bool TryAdd(WorkflowRuntimeException value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            return DoAdd(value) != null;
        }

        Queue<ExpressionBuilder> DoAdd(WorkflowRuntimeException value)
        {
            var key = value.InnerException;
            if (key != null)
            {
                if (key is WorkflowRuntimeException nestedKey)
                {
                    var callStack = DoAdd(nestedKey);
                    if (callStack != null)
                    {
                        callStack.Enqueue(value.Builder);
                        return callStack;
                    }
                }
                else
                {
                    var weakKey = new WeakKey<Exception>(key);
                    if (exceptions.ContainsKey(weakKey)) return null;
                    CollectUnusedExceptions();

                    var callStack = new Queue<ExpressionBuilder>();
                    callStack.Enqueue(value.Builder);
                    exceptions.Add(weakKey, callStack);
                    return callStack;
                }
            }

            return null;
        }

        public bool TryGetValue(Exception key, out WorkflowException value)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            value = null;
            var weakKey = new WeakKey<Exception>(key);
            var result = exceptions.TryGetValue(weakKey, out Queue<ExpressionBuilder> callStack);
            if (result)
            {
                while (callStack.Count > 0)
                {
                    var builder = callStack.Dequeue();
                    value = new WorkflowRuntimeException(key.Message, builder, value ?? key);
                }
            }

            return result;
        }

        public void Clear()
        {
            exceptions.Clear();
        }

        void CollectUnusedExceptions()
        {
            if (--nextCollectionCheck <= 0)
            {
                var keys = exceptions.Keys.ToArray();
                for (int i = 0; i < keys.Length; i++)
                {
                    if (!keys[i].IsAlive)
                    {
                        exceptions.Remove(keys[i]);
                    }
                }

                nextCollectionCheck = CollectionCheck;
            }
        }

        class WeakKey<TKey> where TKey : class
        {
            readonly int hashCode;
            readonly WeakReference<TKey> reference;

            internal WeakKey(TKey key)
            {
                if (key == null)
                {
                    throw new ArgumentNullException(nameof(key));
                }

                hashCode = key.GetHashCode();
                reference = new WeakReference<TKey>(key);
            }

            public bool IsAlive
            {
                get
                {
                    return reference.TryGetTarget(out _);
                }
            }

            public override bool Equals(object obj)
            {
                var weakKey = obj as WeakKey<TKey>;
                if(weakKey.reference == reference) return true;
                if (weakKey != null && hashCode == weakKey.hashCode)
                {
                    if (reference.TryGetTarget(out TKey key1) &&
                        weakKey.reference.TryGetTarget(out TKey key2))
                    {
                        return key1.Equals(key2);
                    }
                }

                return false;
            }

            public override int GetHashCode()
            {
                return hashCode;
            }
        }
    }
}
