namespace Bonsai.Expressions
{
    /// <summary>
    /// Indicates the type of an inspector watch notification.
    /// </summary>
    public enum WatchNotification
    {
        /// <summary>
        /// Indicates the sequence was subscribed to.
        /// </summary>
        Subscribe,

        /// <summary>
        /// Indicates the sequence has emitted a value.
        /// </summary>
        OnNext,

        /// <summary>
        /// Indicates the sequence has terminated with an error.
        /// </summary>
        OnError,

        /// <summary>
        /// Indicates the sequence has terminated successfully.
        /// </summary>
        OnCompleted,

        /// <summary>
        /// Indicates the subscription to the sequence has been disposed.
        /// </summary>
        Unsubscribe
    }
}
