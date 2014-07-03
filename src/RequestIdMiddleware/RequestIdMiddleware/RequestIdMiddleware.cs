namespace RequestIdMiddleware
{
    using System;
    using System.Globalization;
    using System.Threading;
    using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>;
    using MidFunc = System.Func<
        System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>,
        System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>
        >;
    using BuildFunc = System.Action<
        System.Func<
            System.Collections.Generic.IDictionary<string, object>,
            System.Func<
                System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>,
                System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>
                >>>;

    public static class RequestIdMiddleware
    {
        private const string OwinRequestIdKey = "owin.RequestId";
        private static long RequestCounter;

        /// <summary>
        /// Try to set the request ID to a new long.
        /// </summary>
        /// <returns>A builder delegate.</returns>
        public static BuildFunc TrySetRequestIdAsLong(this BuildFunc builder)
        {
            builder(_ => TrySetRequestIdAsLong());
            return builder;
        }

        /// <summary>
        /// Try to set the request ID to a new long.
        /// </summary>
        /// <returns>A middlware delegate.</returns>
        public static MidFunc TrySetRequestIdAsLong()
        {
            return
                TrySetRequestId(() => Interlocked.Increment(ref RequestCounter).ToString(CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Try to set the request ID to a new long with a prefix.
        /// </summary>
        /// <returns>A builder delegate.</returns>
        public static BuildFunc TrySetRequestIdAsLongWithPrefix(this BuildFunc builder, string prefix)
        {
            builder(_ => TrySetRequestIdAsLongWithPrefix(prefix));
            return builder;
        }

        /// <summary>
        /// Try to set the request ID to a new long with a prefix.
        /// </summary>
        /// <returns>A middlware delegate.</returns>
        public static MidFunc TrySetRequestIdAsLongWithPrefix(string prefix)
        {
            if (string.IsNullOrWhiteSpace(prefix))
            {
                throw new ArgumentException("prefix is null or whitespace", "prefix");
            }
            return
                TrySetRequestId(
                    () =>
                        string.Concat(prefix, "-",
                            Interlocked.Increment(ref RequestCounter).ToString(CultureInfo.InvariantCulture)));
        }

        /// <summary>
        /// Try to set the request ID to a new Guid.
        /// </summary>
        /// <returns>A builder delegate.</returns>
        public static BuildFunc TrySetRequestIdAsGuid(this BuildFunc builder)
        {
            builder(_ => TrySetRequestIdAsGuid());
            return builder;
        }

        /// <summary>
        /// Try to set the request ID to a new Guid.
        /// </summary>
        /// <returns>A middlware delegate.</returns>
        public static MidFunc TrySetRequestIdAsGuid()
        {
            return TrySetRequestId(() => Guid.NewGuid().ToString());
        }

        /// <summary>
        /// Try to set the request ID, if it one hasn't already been set.
        /// </summary>
        /// <param name="createRequestId">A delegate to that will generate the request ID</param>
        /// <returns>A builder delegate.</returns>
        public static BuildFunc TrySetRequestId(this BuildFunc builder, Func<string> createRequestId)
        {
            builder(_ => TrySetRequestId(createRequestId));
            return builder;
        }

        /// <summary>
        /// Try to set the request ID, if it one hasn't already been set.
        /// </summary>
        /// <param name="createRequestId">A delegate to that will generate the request ID</param>
        /// <returns>A middlware delegate.</returns>
        public static MidFunc TrySetRequestId(Func<string> createRequestId)
        {
            return
                next =>
                    env =>
                    {
                        if (env.ContainsKey(OwinRequestIdKey) && env[OwinRequestIdKey] != null)
                        {
                            return next(env);
                        }
                        env[OwinRequestIdKey] = createRequestId();
                        return next(env);
                    };
        }
    }
}
