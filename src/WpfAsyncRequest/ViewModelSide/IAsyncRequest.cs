// WpfAsyncRequest
// IAsyncRequest.cs
// Copyright © Timofey Pokhilenko 2018.
// Licensed under the MIT License.
// See LICENSE file in the repository root for full license information.
// https://github.com/ZhakaTaka/wpf-async-request

using System.Threading.Tasks;

namespace WpfAsyncRequest
{
    /// <summary>
    /// Root interface for the ViewModel (sending) side of requests.
    /// Is compatible with all requests and should be used in non-strictly typed solutions.
    /// Derived interfaces help us to distinguish different different types of data and
    /// requests at the ViewModel side.
    /// </summary>
    public interface IAsyncRequest
    {
        Task<object> SendAsync();

        Task<object> SendAsync(object argument);
    }

    /// <summary>
    /// Message, a request without a response.
    /// </summary>
    /// <typeparam name="TMessage">Type of data passed</typeparam>
    public interface IAsyncMessage<in TMessage>
    {
        Task SendAsync();

        Task SendAsync(TMessage message);
    }

    /// <summary>
    /// Pure request. No data passed.
    /// </summary>
    /// <typeparam name="TResponse">Type of response data</typeparam>
    public interface IAsyncRequest<TResponse>
    {
        Task<TResponse> SendAsync();
    }

    /// <summary>
    /// Request which passes some initial data.
    /// </summary>
    /// <typeparam name="TRequest">Type of data passed</typeparam>
    /// <typeparam name="TResponse">Type of response data</typeparam>
    public interface IAsyncRequest<in TRequest, TResponse>
    {
        Task<TResponse> SendAsync();

        Task<TResponse> SendAsync(TRequest argument);
    }
}
