// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See https://github.com/SignalR/SignalR/blob/master/LICENSE.md for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace SignalR.RavenDB
{
    internal static class TaskAsyncHelper
    {
        public static Task Empty = FromResult<object>(null);

        public static Task<T> FromResult<T>(T value)
        {
            var tcs = new TaskCompletionSource<T>();
            tcs.SetResult(value);
            return tcs.Task;
        }

        internal static Task FromError(Exception e)
        {
            return FromError<object>(e);
        }

        internal static Task<T> FromError<T>(Exception e)
        {
            var tcs = new TaskCompletionSource<T>();
            tcs.SetUnwrappedException<T>(e);
            return tcs.Task;
        }

        internal static void SetUnwrappedException<T>(this TaskCompletionSource<T> tcs, Exception e)
        {
            var aggregateException = e as AggregateException;
            if (aggregateException != null)
            {
                tcs.SetException(aggregateException.InnerExceptions);
            }
            else
            {
                tcs.SetException(e);
            }
        }

        internal static bool TrySetUnwrappedException<T>(this TaskCompletionSource<T> tcs, Exception e)
        {
            var aggregateException = e as AggregateException;
            return aggregateException != null 
                ? tcs.TrySetException(aggregateException.InnerExceptions) 
                : tcs.TrySetException(e);
        }
    }
}
