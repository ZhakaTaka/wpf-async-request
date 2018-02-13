// WpfAsyncRequest
// AsyncRequest.AttachedProperty.cs
// Copyright © Timofey Pokhilenko 2018.
// Licensed under the MIT License.
// See LICENSE file in the repository root for full license information.
// https://github.com/ZhakaTaka/wpf-async-request

using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;

namespace WpfAsyncRequest
{
    public partial class AsyncRequest
    {

        private static void SignalAttachedPropertyChanged<TOwner>(TOwner owner, DependencyPropertyChangedEventArgs args) where TOwner : class
        {
            var oldValue = args.OldValue as AsyncRequest;
            var newValue = args.NewValue as AsyncRequest;

            var propertyName = args.Property.Name;

            SignalRegistrationService<TOwner>.UpdateInstanceValue(owner, oldValue, newValue, propertyName);
        }

        #region Untyped

        public static DependencyProperty RegisterAttached<TOwner>(Action<TOwner> reaction, Type declaringType, [CallerMemberName] string propertyName = null) where TOwner : DependencyObject
        {
            if (reaction == null)
            {
                throw new ArgumentNullException(nameof(reaction));
            }

            return RegisterAttached((Func<TOwner, object, object>)((owner, obj) =>
            {
                reaction(owner);
                return null;
            }), declaringType, propertyName);
        }

        public static DependencyProperty RegisterAttached<TOwner>(Action<TOwner, object> reaction, Type declaringType, [CallerMemberName] string propertyName = null) where TOwner : DependencyObject
        {
            if (reaction == null)
            {
                throw new ArgumentNullException(nameof(reaction));
            }

            return RegisterAttached((Func<TOwner, object, object>)((owner, obj) =>
            {
                reaction(owner, obj);
                return null;
            }), declaringType, propertyName);
        }

        public static DependencyProperty RegisterAttached<TOwner>(Func<TOwner, object, object> reaction, Type declaringType, [CallerMemberName] string propertyName = null) where TOwner : DependencyObject
        {
            if (reaction == null)
            {
                throw new ArgumentNullException(nameof(reaction));
            }

            return RegisterAttached<TOwner>((owner, obj) =>
            {
                var completionSource = new TaskCompletionSource<object>();
                owner.Dispatcher.BeginInvoke((Action)(() => completionSource.SetResult(reaction(owner, obj))));
                return completionSource.Task;
            }, declaringType, propertyName);
        }

        public static DependencyProperty RegisterAttached<TOwner>(Func<TOwner, object, Task<object>> reaction, Type declaringType, [CallerMemberName] string propertyName = null) where TOwner : DependencyObject
        {
            propertyName = AcquirePropertyName(propertyName);

            SignalRegistrationService<TOwner>.Register(propertyName, reaction);
            return DependencyProperty.RegisterAttached(propertyName, typeof(IReceivable), declaringType,
                new PropertyMetadata(default(IReceivable), (o, args) => SignalAttachedPropertyChanged((TOwner)o, args)));
        }

        public static DependencyProperty RegisterAttached<TOwner>(Func<TOwner, object, Task> reaction, Type declaringType, [CallerMemberName] string propertyName = null) where TOwner : DependencyObject
        {
            propertyName = AcquirePropertyName(propertyName);

            SignalRegistrationService<TOwner>.RegisterMessage(propertyName, reaction);
            return DependencyProperty.RegisterAttached(propertyName, typeof(IReceivable), declaringType,
                new PropertyMetadata(default(IReceivable), (o, args) => SignalAttachedPropertyChanged((TOwner)o, args)));
        }

        #endregion

        #region Typed

        /// <summary>
        /// Registers an attached request with initial data and a response.
        /// </summary>
        /// <typeparam name="TOwner">Type of DependencyObject which will respond to this request (e.g. Window)</typeparam>
        /// <typeparam name="TRequest">Type of passed data (e.g. string)</typeparam>
        /// <typeparam name="TResult">Type of response data (e.g. bool)</typeparam>
        /// <param name="reaction">An action which will take place and return the result after the request is received</param>
        /// <param name="declaringType">Type which declares this request. (Not the type it will be attached to, but the type of that static class you're currently implementing!)</param>
        /// <param name="propertyName">Name of the property to register. Skip it! It is resovled automatically.</param>
        /// <returns>Registered attached property</returns>
        public static DependencyProperty RegisterAttached<TOwner, TRequest, TResult>(Func<TOwner, TRequest, TResult> reaction, Type declaringType, [CallerMemberName] string propertyName = null) where TOwner : DependencyObject
        {
            if (reaction == null)
            {
                throw new ArgumentNullException(nameof(reaction));
            }

            return RegisterAttached<TOwner, TRequest, TResult>((owner, obj) =>
            {
                var completionSource = new TaskCompletionSource<TResult>();
                owner.Dispatcher.BeginInvoke((Action)(() => completionSource.SetResult(reaction(owner, obj))));
                return completionSource.Task;
            }, declaringType, propertyName);
        }

        /// <summary>
        /// Registers an attached request without initial data and with a response.
        /// </summary>
        /// <typeparam name="TOwner">Type of DependencyObject which will respond to this request (e.g. Window)</typeparam>
        /// <typeparam name="TResult">Type of response data (e.g. bool)</typeparam>
        /// <param name="reaction">An action which will take place and return the result after the request is received</param>
        /// <param name="declaringType">Type which declares this request. (Not the type it will be attached to, but the type of that static class you're currently implementing!)</param>
        /// <param name="propertyName">Name of the property to register. Skip it! It is resovled automatically.</param>
        /// <returns>Registered attached property</returns>
        public static DependencyProperty RegisterAttached<TOwner, TResult>(Func<TOwner, TResult> reaction, Type declaringType, [CallerMemberName] string propertyName = null) where TOwner : DependencyObject
        {
            if (reaction == null)
            {
                throw new ArgumentNullException(nameof(reaction));
            }

            return RegisterAttached<TOwner, TResult>(owner =>
            {
                var completionSource = new TaskCompletionSource<TResult>();
                owner.Dispatcher.BeginInvoke((Action)(() => completionSource.SetResult(reaction(owner))));
                return completionSource.Task;
            }, declaringType, propertyName);
        }

        /// <summary>
        /// Registers an attached request with an initial data and an asynchronous response.
        /// </summary>
        /// <typeparam name="TOwner">Type of DependencyObject which will respond to this request (e.g. Window)</typeparam>
        /// <typeparam name="TRequest">Type of passed data (e.g. string)</typeparam>
        /// <typeparam name="TResult">Type of response data (e.g. bool)</typeparam>
        /// <param name="reaction">An asynchronous action which will take place and return the result after the request is received</param>
        /// <param name="declaringType">Type which declares this request. (Not the type it will be attached to, but the type of that static class you're currently implementing!)</param>
        /// <param name="propertyName">Name of the property to register. Skip it! It is resovled automatically.</param>
        /// <returns>Registered attached property</returns>
        public static DependencyProperty RegisterAttached<TOwner, TRequest, TResult>(Func<TOwner, TRequest, Task<TResult>> reaction, Type declaringType, [CallerMemberName] string propertyName = null) where TOwner : DependencyObject
        {
            if (typeof(TRequest) == typeof(object) && typeof(TResult) == typeof(object))
                return RegisterAttached((Func<TOwner, object, Task<object>>)(object)reaction, declaringType, propertyName);

            propertyName = AcquirePropertyName(propertyName);

            SignalRegistrationService<TOwner>.Register(propertyName, reaction);
            return DependencyProperty.RegisterAttached(propertyName, typeof(IResponsive<TRequest, TResult>), /*typeof(TOwner)*/declaringType,
                new PropertyMetadata(default(IResponsive<TRequest, TResult>),
                    (o, args) => SignalPropertyChanged((TOwner)o, args)));
        }

        /// <summary>
        /// Registers an attached request without initial data and with an asynchronous response.
        /// </summary>
        /// <typeparam name="TOwner">Type of DependencyObject which will respond to this request (e.g. Window)</typeparam>
        /// <typeparam name="TResult">Type of response data (e.g. bool)</typeparam>
        /// <param name="reaction">An asynchronous action which will take place and return the result after the request is received</param>
        /// <param name="declaringType">Type which declares this request. (Not the type it will be attached to, but the type of that static class you're currently implementing!)</param>
        /// <param name="propertyName">Name of the property to register. Skip it! It is resovled automatically.</param>
        /// <returns>Registered attached property</returns>
        public static DependencyProperty RegisterAttached<TOwner, TResult>(Func<TOwner, Task<TResult>> reaction, Type declaringType, [CallerMemberName] string propertyName = null) where TOwner : DependencyObject
        {
            if (typeof(TResult) == typeof(object))
            {
                return RegisterAttached((Func<TOwner, object, Task<object>>)(object)reaction, declaringType, propertyName);
            }

            propertyName = AcquirePropertyName(propertyName);

            SignalRegistrationService<TOwner>.Register(propertyName, reaction);
            return DependencyProperty.RegisterAttached(propertyName, typeof(IResponsive<TResult>), declaringType,
                new PropertyMetadata(default(IResponsive<TResult>),
                    (o, args) => SignalPropertyChanged((TOwner)o, args)));
        }

        /// <summary>
        /// Registers an attached message reaction.
        /// </summary>
        /// <typeparam name="TOwner">Type of DependencyObject which will react this message (e.g. Window)</typeparam>
        /// <typeparam name="TRequest">Type of passed data (e.g. string)</typeparam>
        /// <param name="reaction">An action which will take place after the message is received</param>
        /// <param name="declaringType">Type which declares this message. (Not the type it will be attached to, but the type of that static class you're currently implementing!)</param>
        /// <param name="propertyName">Name of the property to register. Skip it! It is resovled automatically.</param>
        /// <returns>Registered attached property</returns>
        public static DependencyProperty RegisterMessageAttached<TOwner, TRequest>(Action<TOwner, TRequest> reaction, Type declaringType, [CallerMemberName] string propertyName = null) where TOwner : DependencyObject
        {
            if (reaction == null)
            {
                throw new ArgumentNullException(nameof(reaction));
            }

            if (typeof(TRequest) == typeof(object))
            {
                return RegisterAttached((Func<TOwner, object, Task>)(object)reaction, declaringType, propertyName);
            }

            return RegisterMessageAttached<TOwner, TRequest>((owner, request) =>
            {
                var completionSource = new TaskCompletionSource<object>();
                owner.Dispatcher.BeginInvoke((Action)(() =>
                {
                    reaction(owner, request);
                    completionSource.SetResult(null);
                }));
                return completionSource.Task;
            }, declaringType, propertyName);
        }

        /// <summary>
        /// Registers an attached message asynchronous reaction.
        /// </summary>
        /// <typeparam name="TOwner">Type of DependencyObject which will react this message (e.g. Window)</typeparam>
        /// <typeparam name="TRequest">Type of passed data (e.g. string)</typeparam>
        /// <param name="reaction">An asynchronous action which will take place after the message is received</param>
        /// <param name="declaringType">Type which declares this message. (Not the type it will be attached to, but the type of that static class you're currently implementing!)</param>
        /// <param name="propertyName">Name of the property to register. Skip it! It is resovled automatically.</param>
        /// <returns>Registered attached property</returns>
        public static DependencyProperty RegisterMessageAttached<TOwner, TRequest>(Func<TOwner, TRequest, Task> reaction, Type declaringType, [CallerMemberName] string propertyName = null) where TOwner : DependencyObject
        {
            if (typeof(TRequest) == typeof(object))
                return RegisterAttached((Func<TOwner, object, Task>)(object)reaction, declaringType, propertyName);

            propertyName = AcquirePropertyName(propertyName);

            SignalRegistrationService<TOwner>.RegisterMessage(propertyName, reaction);
            return DependencyProperty.RegisterAttached(propertyName, typeof(IMessage<TRequest>), declaringType,
                new PropertyMetadata(default(IMessage<TRequest>),
                    (o, args) => SignalPropertyChanged((TOwner)o, args)));
        }

        #endregion
    }
}
