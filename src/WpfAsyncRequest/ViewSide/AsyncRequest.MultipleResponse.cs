// WpfAsyncRequest
// AsyncRequest.MultipleResponse.cs
// Copyright © Timofey Pokhilenko 2018.
// Licensed under the MIT License.
// See LICENSE file in the repository root for full license information.
// https://github.com/ZhakaTaka/wpf-async-request

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WpfAsyncRequest
{
    public abstract partial class AsyncRequest
    {
        private abstract class MultipleResponseBase : AsyncRequest
        {
            private readonly List<InvocationInfo> _invocationInfos = new List<InvocationInfo>();

            internal override void AddInvocationInfo(InvocationInfo invocationInfo)
            {
                _invocationInfos.Add(invocationInfo);
            }

            protected IEnumerable<InvocationInfo> InvocationInfos
            {
                get
                {
                    var deadOnes = _invocationInfos.Where(info => !info.IsAlive).ToArray();
                    foreach (var invocationInfo in deadOnes)
                    {
                        _invocationInfos.Remove(invocationInfo);
                    }

                    return _invocationInfos;
                }
            }

            internal sealed override void RemoveInvocationInfo(IRegistrationInfo registrationInfo)
            {
                var invocationInfo = _invocationInfos.FirstOrDefault(info => info.RegistrationInfo == registrationInfo);
                if (invocationInfo == null)
                {
                    throw new InvalidOperationException($"Request is unaware of this subscriber: {registrationInfo.OwnerType.Name}.{registrationInfo.PropertyName}");
                }

                _invocationInfos.Remove(invocationInfo);
            }
        }

        private class MultipleResponseAsyncRequest<TRequest, TResult> : MultipleResponseBase, IAsyncRequest<TRequest, IEnumerable<TResult>>, IResponsive<TRequest, TResult>
        {
            public new Task<IEnumerable<TResult>> SendAsync()
            {
                return SendAsync(default(TRequest));
            }

            public async Task<IEnumerable<TResult>> SendAsync(TRequest argument)
            {
                return await Task.WhenAll(InvocationInfos.Select(async info =>
                {
                    var result = await info.InvokeAsync(argument);
                    return (TResult)result;
                }));
            }

            public async override Task<object> SendAsync(object argument)
            {
                var result = await SendAsync((TRequest)argument);
                return result;
            }
        }

        private class MultipleResponseAsyncRequest<TResult> : MultipleResponseBase, IAsyncRequest<IEnumerable<TResult>>, IResponsive<TResult>
        {
            async Task<IEnumerable<TResult>> IAsyncRequest<IEnumerable<TResult>>.SendAsync()
            {
                return await Task.WhenAll(InvocationInfos.Select(async info =>
                {
                    var result = await info.InvokeAsync(null);
                    return (TResult)result;
                }));
            }

            public async override Task<object> SendAsync(object argument)
            {
                var result = await SendAsync();
                return result;
            }
        }

        private class MultipleResponseAsyncMessage<TRequest> : MultipleResponseBase, IAsyncMessage<TRequest>, IMessage<TRequest>
        {
            Task IAsyncMessage<TRequest>.SendAsync()
            {
                return SendAsync(default(TRequest));
            }

            public async Task SendAsync(TRequest argument)
            {
                await Task.WhenAll(InvocationInfos.Select(async info =>
                {
                    await info.InvokeAsync(argument);
                }));
            }

            public async override Task<object> SendAsync(object argument)
            {
                await SendAsync((TRequest)argument);
                return null;
            }
        }

    }
}
