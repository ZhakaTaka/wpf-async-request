### WPF Async Request
A solution for the problem of sending user interaction requests from ViewModel.  
Doesn't use and doesn't require you to use ANYTHING except pure WPF.  
Loose coupling, follows MVVM.  
Uses Tasks and encourages usage of async/await.  
Versions for other UI frameworks are planned (I hope I'll have enough time :))  
  
#### Usage

1. **Define a Request Source.** Define a get-only property of type **IAsyncRequest** or **IMessage** in your *ViewModel*.  
   Property type and generic arguments depend on your requirements:
   * Use *IAsyncRequest&lt;TResponse&gt;* if you want to receive some data from user (e.g. slect a file to open)
   * Use *IAsyncMessage&lt;TMessage&gt;* if you want to notify user but do not need to receive anything (e.g. MessageBox with just one Ok button)
   * Use *IAsyncRequest&lt;TRequest, TResponse&gt;* if you want to send some data (*TRequest*) and then receive some answer (*TResponse*)  
     (e.g. MessageBox with multiple buttons like in VerySimpleDemo project)  
     ***Note: All exapmles will be given with this case.***
   
   **Initialize your property.** You have two options:  
   * Single response:   
     You have one any the only listener on the *View* side. (An Exception will be thrown in case of multiple listeners)  
     `IAsyncRequest<TRequest, TResult> _myRequest = AsyncRequest.CreateSingleResponseRequest<TRequest, TResult>();`  
     
   * Multiple response:  
     You can have multiple listeners. An *IEnumerable&lt;TResponse&gt;* is returned as the result.  
     `IAsyncRequest<TRequest, IEnumerable<TResult>> _myRequest = AsyncRequest.CreateMultipleResponseRequest<TRequest, TResult>();`  

1. **Define a Reaction.** Define the the reaction part on the *View* side. You have two options:
   * *AttachedProperty* (more preferrable)  
     Can be used with any control, highly flexible and reusable solution.  
        
   * *DependencyProperty* (less preferrable)  
     Use it if you create your own control which you plan to be able to react requests.  
     This approach less flexible and a reaction code is unlikely to be reused.  
     You have an access to your control's private members however.

   
   ##### AttachedProperty HowTo
   Within some static class (like you always do with an *AttachedProperty*) write the following code:  
   ```
       public static readonly DependencyProperty MyRequestProperty
           = AsyncRequest.RegisterAttached((Func<TControl, TRequest, TResponse>)ReactionMethod, typeof(DeclaringClass));
      
       public static void SetMyRequest(TControl control, TReaction value)
       {
           control.SetValue(MyRequestProperty, value);
       }
      
       public static TReacion GetMyRequest(TControl control)
       {
           return (TReacion)control.GetValue(MyRequestProperty);
       }

       private static TReponse ReactionMethod(TControl control, TRequest request)
       {
           // React somehow. Don't forget to return the response.
       }

   ```
   **Where**  
   *MyRequest* is a name of the request.  
   *TControl* is a type of the control you want to react to your request.  
   *DeclaringClass* is a type of the class hosting your property.  
   *TReaction* is a type of the reaction property and is defined by the type of your request as following:  

   | Request type                               | Reaction property type                   |
   |--------------------------------------------|------------------------------------------|
   | *IAsyncRequest&lt;TResponse&gt;*           | *IResponsive&lt;TResponse&gt;*           |
   | *IAsyncMessage&lt;TMessage&gt;*            | *IMessage&lt;TMessage&gt;*               |
   | *IAsyncRequest&lt;TRequest, TResponse&gt;* | *IResponsive&lt;TRequest, TResponse&gt;* |
 
   *ReactionMethod* is a reaction itself. It can be async and/or return *Task&lt;TResponse&gt;* also.  
     
   *For the DependencyProperty option use `AsyncRequest.RegisterDependency` within your control class.
   Everything else is pretty much the same.*
 1. **Bind**  
    Inside of the control tag **bind** your property(using OneWayBindong of course) to the request property of your ViewModel.
    ```
    <SomeControl local:DeclaringClass.MyRequest="{Binding MyRequest}" />
    ```
    Or use propgrammatic binding if required.

 1. **Invoke**  
    In your ViewModel invoke your Request as follows:  
    `var result = await MyRequest.SendAsync(requestData);`  
    *If the property isn't bound, **null** is returned immediately.*

That's it. Four simple steps:
1. **Define a Request Source**
1. **Define a Reaction**
1. **Bind**
1. **Invoke**

*If you're sending a request from a non-UI thread, relax and don't think about it. Requests will do everything for you.*  
*Weak references are used, so the request doesn't prevent the View from being collected.*
