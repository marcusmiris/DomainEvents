using System;
using System.Collections;
using System.Collections.Generic;
using System.Transactions;

namespace Miris.DomainEvents.ImmediateConsistency
{
    /// <summary>
    ///     Wrapper para <see cref="TransactionInformation.LocalIdentifier"/>.
    /// </summary>
    /// <remarks>
    ///     A implementação foi inspirada em <see cref="Tuple{T1}"/>.
    ///     https://referencesource.microsoft.com/mscorlib/system/tuple.cs.html#25b439d8dd9b7594
    /// </remarks>
    internal class TransactionId
        : IStructuralEquatable
        , IStructuralComparable
        , IComparable
    {
        public string Value { get; }

        #region ' Constructor '

        public TransactionId(Transaction transaction)
        {
            if (transaction == null) throw new ArgumentNullException(nameof(transaction));

            var localIdentifer = transaction.TransactionInformation.LocalIdentifier;

            Value = $"{ localIdentifer }";
        }

        #endregion

        #region ' Equais / GetHashCode / ToString overrides '

        #region ' IStructuralEquatable '

        bool IStructuralEquatable.Equals(object other, IEqualityComparer comparer)
        {
            if (other == null) return false;

            var objTuple = other as TransactionId;

            return objTuple != null 
                && comparer.Equals(Value, objTuple.Value);
        }

        int IStructuralEquatable.GetHashCode(IEqualityComparer comparer) => comparer.GetHashCode(Value);

        #endregion

        #region ' IStructuralComparable '

        int IStructuralComparable.CompareTo(object other, IComparer comparer)
        {
            if (other == null) return 1;

            var objTuple = other as TransactionId;
            if (objTuple == null)
                throw new ArgumentException($"Argumento precisa ser do tipo { GetType() }", nameof(other));

            return comparer.Compare(Value, objTuple.Value);
        }

        #endregion

        #region ' IComparable '

        int IComparable.CompareTo(object obj)
            => ((IStructuralComparable)this).CompareTo(obj, Comparer<object>.Default);

        #endregion

        public override bool Equals(object obj)
            => ((IStructuralEquatable)this).Equals(obj, EqualityComparer<Object>.Default);

        public override int GetHashCode()
            => ((IStructuralEquatable)this).GetHashCode(EqualityComparer<object>.Default);

        #endregion

        public override string ToString() => Value;

    }
}
