// WpfAsyncRequest
// AsyncRequest.SingleResponse.cs
// Copyright © Timofey Pokhilenko 2018.
// Licensed under the MIT License.
// See LICENSE file in the repository root for full license information.
// https://github.com/ZhakaTaka/wpf-async-request

using System;
using System.Threading.Tasks;

namespace WpfAsyncRequest
{
    public abstract partial class AsyncRequest
    {
        private abstract class SingleResponseBase : AsyncRequest
        {
            protected new InvocationInfo InvocationInfo { get; private set; }

            internal override void AddInvocationInfo(InvocationInfo invocationInfo)
            {
                if (InvocationInfo != null)
                {
                    throw new NotSupportedException("Multiple reactions are not supported for single response AsyncRequest. Use multiple response version instead. " +
                                                    $"{invocationInfo.OwnerType.Name}.{invocationInfo.RegistrationInfo.PropertyName} is already registered for this request.");
                }

                InvocationInfo = invocationInfo;
            }

            internal override void RemoveInvocationInfo(IRegistrationInfo registrationInfo)
            {
                if (InvocationInfo == null || InvocationInfo.RegistrationInfo != registrationInfo)
                {
                    throw new InvalidOperationException($"The request is unaware of this subscriber: {registrationInfo.OwnerType.Name}.{registrationInfo.PropertyName}");
                }

                InvocationInfo = null;
            }
        }

        private class SingleResponseAsyncRequest<TRequest, TResult> : SingleResponseBase, IAsyncRequest<TRequest, TResult>, IResponsive<TRequest, TResult>
        {
            public new Task<TResult> SendAsync()
            {
                return SendAsync(default(TRequest));
            }

            public async override Task<object> SendAsync(object argument)
            {
                var result = await SendAsync((TRequest)argument);
                return result;
            }

            public async Task<TResult> SendAsync(TRequest argument)
            {
                var invocationInfo = InvocationInfo;
                if (invocationInfo != null)
                {
                    var result = await invocationInfo.InvokeAsync(argument);
                    return (TResult)result;
                }

                return default(TResult);
            }
        }

        private class SingleResponseAsyncRequest<TResult> : SingleResponseBase, IAsyncRequest<TResult>, IResponsive<TResult>
        {
            public async override Task<object> SendAsync(object argument)
            {
                var result = await SendAsync();
                return result;
            }

            async Task<TResult> IAsyncRequest<TResult>.SendAsync()
            {
                var invocationInfo = InvocationInfo;
                if (invocationInfo != null)
                {
                    var result = await invocationInfo.InvokeAsync(null);
                    return (TResult)result;
                }

                return default(TResult);
            }
        }

        private class SingleResponseAsyncMessage<TRequest> : SingleResponseBase, IAsyncMessage<TRequest>, IMessage<TRequest>
        {
            public async override Task<object> SendAsync(object argument)
            {
                await SendAsync((TRequest)argument);
                return null;
            }

            Task IAsyncMessage<TRequest>.SendAsync()
            {
                return SendAsync(default(TRequest));
            }

            public async Task SendAsync(TRequest argument)
            {
                var invocationInfo = InvocationInfo;
                if (invocationInfo != null)
                {
                    await invocationInfo?.InvokeAsync(argument);
                }
            }
        }
    }
}
