// WpfAsyncRequest
// IReceivable.cs
// Copyright © Timofey Pokhilenko 2018.
// Licensed under the MIT License.
// See LICENSE file in the repository root for full license information.
// https://github.com/ZhakaTaka/wpf-async-request

namespace WpfAsyncRequest
{
    /// <summary>
    /// Root interface for the View (receiving) side of requests.
    /// It is compatible with all requests and should be used in non-strictly typed solutions.
    /// Derived interfaces help us to distinguish different different types of data and requests at
    /// the View side.
    /// While IAsyncRequest counterparts wil most likely work on both sides it's strongly recommended
    /// to use these IReceivable and its derivatives on the View side because the View shouldn't have
    /// any control (i.e. send the request) and have a passive role in this construct.
    /// </summary>
    public interface IReceivable { }

    /// <summary>
    /// Message, a request without a response.
    /// </summary>
    /// <typeparam name="TRequest">Type of data passed</typeparam>
    public interface IMessage<out TRequest> : IReceivable { }

    /// <summary>
    /// Pure request. No data passed.
    /// </summary>
    /// <typeparam name="TResponse">Type of response data</typeparam>
    public interface IResponsive<in TResponse> : IReceivable { }

    /// <summary>
    /// Request which passes some initial data.
    /// </summary>
    /// <typeparam name="TRequest">Type of data passed</typeparam>
    /// <typeparam name="TResponse">Type of response data</typeparam>
    public interface IResponsive<out TRequest, in TResponse> : IResponsive<TResponse>, IMessage<TRequest> { }
}
