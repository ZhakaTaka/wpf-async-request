// WpfAsyncRequest
// AsyncRequest.cs
// Copyright © Timofey Pokhilenko 2018.
// Licensed under the MIT License.
// See LICENSE file in the repository root for full license information.
// https://github.com/ZhakaTaka/wpf-async-request

using System.Threading.Tasks;

namespace WpfAsyncRequest
{
    public abstract partial class AsyncRequest : IAsyncRequest, IReceivable
    {
        internal abstract void AddInvocationInfo(InvocationInfo invocationInfo);

        internal abstract void RemoveInvocationInfo(IRegistrationInfo registrationInfo);

        public Task<object> SendAsync()
        {
            return SendAsync(null);
        }

        public abstract Task<object> SendAsync(object argument);
    }
}
