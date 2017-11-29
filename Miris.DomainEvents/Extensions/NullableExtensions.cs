using System;
using System.Diagnostics;

namespace Miris.DomainEvents.Extensions
{
    public static class NullableExtensions
    {

        #region ' GetValueOrThrowsException(...) '

        [DebuggerStepThrough]
        public static T GetValueOrThrowsException<T>(this T t, string exceptionMessage)
            where T : class 
            => t ?? throw new NullReferenceException(exceptionMessage);

        #endregion

    }
}
