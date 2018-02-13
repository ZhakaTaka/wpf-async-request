// WpfAsyncRequest
// AsyncRequest.DependencyProperty.cs
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
        private static string AcquirePropertyName(string fieldName)
        {
            const string propertySuffix = "Property";

            return fieldName.EndsWith(propertySuffix)
                ? fieldName.Remove(fieldName.Length - propertySuffix.Length)
                : fieldName;
        }

        private static void SignalPropertyChanged<TOwner>(TOwner owner, DependencyPropertyChangedEventArgs args) where TOwner : DependencyObject
        {
            var oldValue = args.OldValue as AsyncRequest;
            var newValue = args.NewValue as AsyncRequest;

            var propertyName = args.Property.Name;

            SignalRegistrationService<TOwner>.UpdateInstanceValue(owner, oldValue, newValue, propertyName);
        }

        #region Untyped

        public static DependencyProperty Register<TOwner>(Action<TOwner> reaction, [CallerMemberName] string propertyName = null) where TOwner : DependencyObject
        {
            if (reaction == null)
            {
                throw new ArgumentNullException(nameof(reaction));
            }

            return Register((Func<TOwner, object, object>)((owner, obj) =>
            {
                reaction(owner);
                return null;
            }), propertyName);
        }

        /// <summary>
        /// Used to register message request.
        /// </summary>
        /// <typeparam name="TOwner">Owner type</typeparam>
        /// <param name="reaction">Message reaction</param>
        /// <param name="propertyName">DependencyProperty name</param>
        /// <returns></returns>
        public static DependencyProperty Register<TOwner>(Action<TOwner, object> reaction, [CallerMemberName] string propertyName = null) where TOwner : DependencyObject
        {
            if (reaction == null)
            {
                throw new ArgumentNullException(nameof(reaction));
            }

            return Register((Func<TOwner, object, object>) ((owner, obj) =>
            {
                reaction(owner, obj);
                return null;
            }), propertyName);
        }

        /// <summary>
        /// Used to register parameterless request.
        /// </summary>
        /// <typeparam name="TOwner">Owner type</typeparam>
        /// <param name="reaction">Request reaction</param>
        /// <param name="propertyName">DependencyProperty name</param>
        /// <returns></returns>
        public static DependencyProperty Register<TOwner>(Func<TOwner, object> reaction, [CallerMemberName] string propertyName = null) where TOwner : DependencyObject
        {
            if (reaction == null)
            {
                throw new ArgumentNullException(nameof(reaction));
            }

            return Register((Func<TOwner, object, object>)((owner, obj) => reaction(owner)), propertyName);
        }

        public static DependencyProperty Register<TOwner>(Func<TOwner, object, object> reaction, [CallerMemberName] string propertyName = null) where TOwner : DependencyObject
        {
            if (reaction == null)
            {
                throw new ArgumentNullException(nameof(reaction));
            }

            return Register<TOwner>((owner, obj) =>
            {
                var completionSource = new TaskCompletionSource<object>();

                owner.Dispatcher.BeginInvoke((Action) (() => completionSource.SetResult(reaction(owner, obj))));

                return completionSource.Task;
            }, propertyName);
        }


        public static DependencyProperty Register<TOwner>(Func<TOwner, Task<object>> reaction,
            [CallerMemberName] string propertyName = null) where TOwner : DependencyObject
        {
            if (reaction == null)
            {
                throw new ArgumentNullException(nameof(reaction));
            }

            return Register(((TOwner owner, object obj) => reaction(owner)), propertyName);
        }

        public static DependencyProperty Register<TOwner>(Func<TOwner, object, Task<object>> reaction, [CallerMemberName] string propertyName = null) where TOwner : DependencyObject
        {
            propertyName = AcquirePropertyName(propertyName);

            SignalRegistrationService<TOwner>.Register(propertyName, reaction);
            return DependencyProperty.Register(propertyName, typeof (IReceivable), typeof (TOwner),
                new PropertyMetadata(default(IReceivable), (o, args) => SignalPropertyChanged((TOwner) o, args)));
        }

        public static DependencyProperty Register<TOwner>(Func<TOwner, object, Task> reaction, [CallerMemberName] string propertyName = null) where TOwner : DependencyObject
        {
            propertyName = AcquirePropertyName(propertyName);

            SignalRegistrationService<TOwner>.RegisterMessage(propertyName, reaction);
            return DependencyProperty.Register(propertyName, typeof(IReceivable), typeof(TOwner),
                new PropertyMetadata(default(IReceivable), (o, args) => SignalPropertyChanged((TOwner)o, args)));
        }

        #endregion

        #region Typed


        public static DependencyProperty Register<TOwner, TRequest, TResult>(
            Func<TOwner, TRequest, TResult> reaction,
            [CallerMemberName] string propertyName = null)
            where TOwner : DependencyObject
        {
            if (reaction == null)
            {
                throw new ArgumentNullException(nameof(reaction));
            }

            return Register<TOwner, TRequest, TResult>((owner, obj) =>
            {
                var completionSource = new TaskCompletionSource<TResult>();

                owner.Dispatcher.BeginInvoke((Action) (() => completionSource.SetResult(reaction(owner, obj))));

                return completionSource.Task;
            }, propertyName);
        }

        public static DependencyProperty Register<TOwner, TResult>(
            Func<TOwner, TResult> reaction,
            [CallerMemberName] string propertyName = null)
            where TOwner : DependencyObject
        {
            if (reaction == null)
            {
                throw new ArgumentNullException(nameof(reaction));
            }

            return Register<TOwner, TResult>((owner) =>
            {
                var completionSource = new TaskCompletionSource<TResult>();

                owner.Dispatcher.BeginInvoke((Action)(() => completionSource.SetResult(reaction(owner))));

                return completionSource.Task;
            }, propertyName);
        }

        public static DependencyProperty Register<TOwner, TRequest, TResult>(
            Func<TOwner, TRequest, Task<TResult>> reaction,
            [CallerMemberName] string propertyName = null)
            where TOwner : DependencyObject
        {
            if (typeof(TRequest) == typeof(object) && typeof(TResult) == typeof(object))
            {
                return Register((Func<TOwner, object, Task<object>>)(object)reaction, propertyName);
            }

            propertyName = AcquirePropertyName(propertyName);

            SignalRegistrationService<TOwner>.Register(propertyName, reaction);
            return DependencyProperty.Register(
                propertyName,
                typeof (IResponsive<TRequest, TResult>),
                typeof (TOwner),
                new PropertyMetadata(
                    default(IResponsive<TRequest, TResult>),
                    (o, args) => SignalPropertyChanged((TOwner) o, args)));
        }

        public static DependencyProperty Register<TOwner, TResult>(
            Func<TOwner, Task<TResult>> reaction,
            [CallerMemberName] string propertyName = null)
            where TOwner : DependencyObject
        {
            if (typeof(TResult) == typeof(object))
            {
                return Register((Func<TOwner, Task<object>>)(object)reaction, propertyName);
            }

            propertyName = AcquirePropertyName(propertyName);

            SignalRegistrationService<TOwner>.Register(propertyName, reaction);
            return DependencyProperty.Register(
                propertyName,
                typeof(IResponsive<TResult>),
                typeof(TOwner),
                new PropertyMetadata(
                    default(IResponsive<TResult>),
                    (o, args) => SignalPropertyChanged((TOwner)o, args)));
        }

        public static DependencyProperty RegisterMessage<TOwner, TRequest>(
            Func<TOwner, TRequest, Task> reaction,
            [CallerMemberName] string propertyName = null)
            where TOwner : DependencyObject
        {
            if (typeof(TRequest) == typeof(object))
            {
                return Register((Func<TOwner, object, Task>)(object)reaction, propertyName);
            }

            propertyName = AcquirePropertyName(propertyName);

            SignalRegistrationService<TOwner>.RegisterMessage(propertyName, reaction);
            return DependencyProperty.Register(
                propertyName,
                typeof(IMessage<TRequest>),
                typeof(TOwner),
                new PropertyMetadata(
                    default(IMessage<TRequest>),
                    (o, args) => SignalPropertyChanged((TOwner)o, args)));
        }

        #endregion
    }
}
