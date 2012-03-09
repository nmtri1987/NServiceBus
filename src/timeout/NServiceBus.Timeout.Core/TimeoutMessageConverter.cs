namespace NServiceBus.Timeout.Core
{
    using System.Globalization;
    using Config;
    using MessageMutator;
    using Saga;

    public class TimeoutMessageConverter : IMessageMutator, INeedInitialization
    {
        public IBus Bus { get; set; }

        /// <summary>
        /// Set the headers for outgoing messages to make sure that the 3.0 timeout manager will work correctly
        /// </summary>
        /// <param name="message"></param>
        public object MutateOutgoing(object message)
        {
            PromoteHeaders(message);

            return message;
        }

   
        void PromoteHeaders(object message)
        {
            var timeoutMessage = message as TimeoutMessage;

            if (timeoutMessage == null)
                return;

            Bus.CurrentMessageContext.Headers[Headers.SagaId] = timeoutMessage.SagaId.ToString();
            Bus.CurrentMessageContext.Headers[Headers.Expire] = timeoutMessage.Expires.ToWireFormattedString();

            if (timeoutMessage.ClearTimeout)
                Bus.CurrentMessageContext.Headers[Headers.ClearTimeouts] = true.ToString(CultureInfo.InvariantCulture);

        }


        /// <summary>
        /// We set the values for incoming messages as well so that we are backwards compatible with 2.6 timeouts
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public object MutateIncoming(object message)
        {
            PromoteHeaders(message);

            return message;
        }

        public void Init()
        {
            Configure.Instance.Configurer.ConfigureComponent<TimeoutMessageConverter>(DependencyLifecycle.InstancePerCall);
        }

    }
}