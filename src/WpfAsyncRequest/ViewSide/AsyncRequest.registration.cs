// WpfAsyncRequest
// AsyncRequest.registration.cs
// Copyright © Timofey Pokhilenko 2018.
// Licensed under the MIT License.
// See LICENSE file in the repository root for full license information.
// https://github.com/ZhakaTaka/wpf-async-request

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;

namespace WpfAsyncRequest
{
    public abstract partial class AsyncRequest
    {
        internal class InvocationInfo
        {
            private readonly WeakReference _ownerRef;
            public readonly IRegistrationInfo RegistrationInfo;

            public Type OwnerType
            {
                get
                {
                    var owner = _ownerRef.Target;
                    return owner == null ? null : owner.GetType();
                }
            }

            public InvocationInfo(object owner, IRegistrationInfo registrationInfo)
            {
                _ownerRef = new WeakReference(owner);
                RegistrationInfo = registrationInfo;
            }

            public bool IsAlive { get { return _ownerRef.IsAlive; } }

            public Task<object> InvokeAsync(object data)
            {
                var owner = _ownerRef.Target;
                return owner == null
                    ? Task.FromResult<object>(null)
                    : RegistrationInfo.InvokeReaction(owner, data);
            }
        }

        internal interface IRegistrationInfo
        {
            string PropertyName { get; }
            Task<object> InvokeReaction(object owner, object requestData);
            Type OwnerType { get; }
        }

        internal static class SignalRegistrationService<TOwner> where TOwner : class
        {
            private static string GetInvocationInfoName(string propertyName, Type declaringType)
            {
                return string.Format("{0}:{1}", declaringType.FullName, propertyName);
            }

            private static readonly Dictionary<string, RegistrationInfo> RegistrationEntries = new Dictionary<string, RegistrationInfo>();

            public static void RegisterAttached(string propertyName, Type declaringType,
                Func<TOwner, object, Task<object>> reaction)
            {
                Register(GetInvocationInfoName(propertyName, declaringType), reaction);
            }

            public static void RegisterAttached<TRequest, TResult>(string propertyName, Type declaringType,
                Func<TOwner, TRequest, Task<TResult>> reaction)
            {
                Register(GetInvocationInfoName(propertyName, declaringType), reaction);
            }

            public static void Register(string propertyName, Func<TOwner, object, Task<object>> reaction)
            {
                var registrationInfo = new RegistrationInfoForObjectObject(propertyName, reaction);
                RegistrationEntries[propertyName] = registrationInfo;
            }


            public static void Register<TRequest, TResult>(string propertyName, Func<TOwner, TRequest, Task<TResult>> reaction)
            {
                var registrationInfo = new RegistrationInfo<TRequest, TResult>(propertyName, reaction);
                RegistrationEntries[propertyName] = registrationInfo;
            }

            public static void Register<TResult>(string propertyName, Func<TOwner, Task<TResult>> reaction)
            {
                var registrationInfo = new RegistrationInfo<TResult>(propertyName, reaction);
                RegistrationEntries[propertyName] = registrationInfo;
            }

            public static void RegisterMessage<TRequest>(string propertyName, Func<TOwner, TRequest, Task> reaction)
            {
                var registrationInfo = new MessageRegistrationInfo<TRequest>(propertyName, reaction);
                RegistrationEntries[propertyName] = registrationInfo;
            }

            #region RegistrationInfo

            private abstract class RegistrationInfo : IRegistrationInfo
            {
                public string PropertyName { get; private set; }
                public abstract Task<object> InvokeReaction(object owner, object requestData);
                public Type OwnerType { get { return typeof(TOwner); } }

                protected RegistrationInfo(string propertyName)
                {
                    PropertyName = propertyName;
                }
            }

            //For optimization purposes: cast-free execution
            private class RegistrationInfoForObjectObject : RegistrationInfo
            {
                private readonly Func<TOwner, object, Task<object>> _reaction;

                public override Task<object> InvokeReaction(object owner, object requestData)
                {
                    return _reaction((TOwner)owner, requestData);
                }

                public RegistrationInfoForObjectObject(string propertyName, Func<TOwner, object, Task<object>> reaction)
                    : base(propertyName)
                {
                    _reaction = reaction;
                }
            }

            private class RegistrationInfo<TRequest, TResult> : RegistrationInfo
            {
                private readonly Func<TOwner, TRequest, Task<TResult>> _reaction;

                public async override Task<object> InvokeReaction(object owner, object requestData)
                {
                    var dispatcher = ((DependencyObject) owner).Dispatcher;

                    if (dispatcher.CheckAccess())
                        return await _reaction((TOwner) owner, (TRequest) requestData);
                    
                    var completionSource = new TaskCompletionSource<TResult>();
                    dispatcher.BeginInvoke(new Action(async () => completionSource.SetResult(await _reaction((TOwner) owner, (TRequest) requestData))));
                    return await completionSource.Task;
                }

                public RegistrationInfo(string propertyName, Func<TOwner, TRequest, Task<TResult>> reaction)
                    : base(propertyName)
                {
                    _reaction = reaction;
                }
            }

            private class RegistrationInfo<TResult> : RegistrationInfo
            {
                private readonly Func<TOwner, Task<TResult>> _reaction;

                public async override Task<object> InvokeReaction(object owner, object requestData)
                {
                    var dispatcher = ((DependencyObject)owner).Dispatcher;

                    if (dispatcher.CheckAccess())
                        return await _reaction((TOwner)owner);

                    var completionSource = new TaskCompletionSource<TResult>();
                    dispatcher.BeginInvoke(new Action(async () => completionSource.SetResult(await _reaction((TOwner)owner))));
                    return await completionSource.Task;
                }

                public RegistrationInfo(string propertyName, Func<TOwner, Task<TResult>> reaction)
                    : base(propertyName)
                {
                    _reaction = reaction;
                }
            }

            private class MessageRegistrationInfo<TRequest> : RegistrationInfo
            {
                private readonly Func<TOwner, TRequest, Task> _reaction;

                public async override Task<object> InvokeReaction(object owner, object requestData)
                {
                    var dispatcher = ((DependencyObject)owner).Dispatcher;

                    if (dispatcher.CheckAccess())
                    {
                        await _reaction((TOwner) owner, (TRequest) requestData);
                        return null;
                    }

                    var completionSource = new TaskCompletionSource<bool>();
                    dispatcher.BeginInvoke(new Action(async () =>
                    {
                        await _reaction((TOwner) owner, (TRequest) requestData);
                        completionSource.SetResult(true);
                    }));
                    return await completionSource.Task;
                }

                public MessageRegistrationInfo(string propertyName, Func<TOwner, TRequest, Task> reaction)
                    : base(propertyName)
                {
                    _reaction = reaction;
                }
            }

            #endregion

            public static void UpdateInstanceValue(TOwner owner, AsyncRequest oldValue, AsyncRequest newValue, string propertyName)
            {
                var entry = RegistrationEntries[propertyName];
                if (oldValue != null)
                    oldValue.RemoveInvocationInfo(entry);

                if (newValue != null)
                    newValue.AddInvocationInfo(new InvocationInfo(owner, entry));
            }

            public static void UpdateInstanceValueAttached(TOwner owner, AsyncRequest oldValue, AsyncRequest newValue, string propertyName)
            {
                var entry = RegistrationEntries[propertyName];
                if (oldValue != null)
                    oldValue.RemoveInvocationInfo(entry);

                if (newValue != null)
                    newValue.AddInvocationInfo(new InvocationInfo(owner, entry));
            }
        }

    }
}
