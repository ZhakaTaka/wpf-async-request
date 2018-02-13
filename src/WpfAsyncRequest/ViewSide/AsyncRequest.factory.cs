// WpfAsyncRequest
// AsyncRequest.factory.cs
// Copyright © Timofey Pokhilenko 2018.
// Licensed under the MIT License.
// See LICENSE file in the repository root for full license information.
// https://github.com/ZhakaTaka/wpf-async-request

using System.Collections.Generic;

namespace WpfAsyncRequest
{
    public abstract partial class AsyncRequest
    {
        public static IAsyncRequest CreateSingleResponseRequest()
        {
            return new SingleResponseAsyncRequest<object, object>();
        }

        public static IAsyncRequest CreateMultipleResponseRequest()
        {
            return new MultipleResponseAsyncRequest<object, object>();
        }

        public static IAsyncRequest<TRequest, TResult> CreateSingleResponseRequest<TRequest, TResult>()
        {
            return new SingleResponseAsyncRequest<TRequest, TResult>();
        }

        public static IAsyncRequest<TResult> CreateSingleResponseRequest<TResult>()
        {
            return new SingleResponseAsyncRequest<TResult>();
        }

        public static IAsyncMessage<TRequest> CreateSingleResponseMessage<TRequest>()
        {
            return new SingleResponseAsyncMessage<TRequest>();
        }

        public static IAsyncRequest<TRequest, IEnumerable<TResult>> CreateMultipleResponseRequest<TRequest, TResult>()
        {
            return new MultipleResponseAsyncRequest<TRequest, TResult>();
        }

        public static IAsyncRequest<IEnumerable<TResult>> CreateMultipleResponseRequest<TResult>()
        {
            return new MultipleResponseAsyncRequest<TResult>();
        }

        public static IAsyncMessage<TRequest> CreateMultipleResponseMessage<TRequest>()
        {
            return new MultipleResponseAsyncMessage<TRequest>();
        }

    }
}
