using System;
using NaughtyAttributes;
using UnityEngine;

namespace Code.Runtime.Statistics
{
    [Serializable]
    public sealed class Resource : Stat, IResource
    {
        public Resource( StatType resource, /*Stat regen,*/ float baseValue ) : base( resource, baseValue )
        {
            CurrentValue = baseValue;
            TotalValue.OnTotalChanged += _ => SetCurrentTo( CurrentValue );
        }

        //Regen = regen;
        [field: SerializeField, ReadOnly] public float CurrentValue { get; private set; }
        //[field: SerializeField, ReadOnly] public Stat Regen { get; private set; }

        public bool IsDepleted => CurrentValue <= 0;
        public bool IsFull => CurrentValue >= TotalValue;
        public float MissingValue => TotalValue - CurrentValue;
        public float Percentage => CurrentValue / TotalValue;
        public event Action<float, float, float> OnCurrentChanged; // (previous, newValue, total)
        public event Action OnDepleted;
        public event Action OnRecharged;

        public bool CanSpend( float amount ) => StatType == StatType.MaxLife
            ? amount < CurrentValue // prevent deplete health when using health as a resource
            : amount <= CurrentValue;

        /// <summary>Tries to add the amount to the current value.</summary>
        /// <returns>The remaining amount that was not added</returns>
        public float IncreaseCurrent( float amountToAdd )
        {
            if( amountToAdd < 0 )
                throw new ArgumentOutOfRangeException( nameof( amountToAdd ), "Amount to add must be positive" );

            var added = Math.Min( MissingValue, amountToAdd );

            if( added != 0 )
                SetCurrentTo( CurrentValue + added );

            return amountToAdd - added;
        }

        //public void Regenerate( float tickRate ) => _ = IncreaseCurrent( Regen * tickRate );

        /// <summary>Tries to remove the amount from the current value</summary>
        /// <returns>The remaining amount that was not removed</returns>
        public float ReduceCurrent( float amountToRemove )
        {
            if( amountToRemove < 0 )
                throw new ArgumentOutOfRangeException( nameof( amountToRemove ), "Amount to remove must be positive" );

            var removed = Math.Min( CurrentValue, amountToRemove );

            if( removed != 0 )
                SetCurrentTo( CurrentValue - removed );

            return amountToRemove - removed;
        }

        public void RefillCurrent() => SetCurrentTo( TotalValue );
        //public void DepleteCurrent() => SetCurrentTo(0);

        public override Stat GetDeepCopy()
        {
            var other = (Resource) MemberwiseClone();
            other.name = string.Copy( name );
            other.StatType = StatType;
            other.TotalValue = TotalValue;
            other.CurrentValue = CurrentValue;
            other.OnCurrentChanged = null; //have no listeners to these deep copies

            return other;
        }

        private void SetCurrentTo( float value )
        {
            var newCurrent = Mathf.Clamp( value, 0, TotalValue );

            if( Mathf.Approximately( CurrentValue, newCurrent ) )
                return;

            OnCurrentChanged?.Invoke( CurrentValue, newCurrent, TotalValue );

            CurrentValue = newCurrent;

            if( IsDepleted )
                OnDepleted?.Invoke();
            else if( IsFull )
                OnRecharged?.Invoke();
        }

        public Resource GetResourceCopy()
        {
            var other = (Resource) MemberwiseClone();
            other.name = string.Copy( name );
            other.StatType = StatType;
            other.TotalValue = TotalValue;
            other.OnCurrentChanged = null; //have no listeners to these deep copies

            return other;
        }
    }

    internal interface IResource : IStat
    {
        float CurrentValue { get; }
        bool IsDepleted { get; }
        bool IsFull { get; }
        float MissingValue { get; }

        event Action<float, float, float> OnCurrentChanged;
        event Action OnDepleted;
        event Action OnRecharged;

        bool CanSpend( float amount );
        float IncreaseCurrent( float amountToAdd );

        float ReduceCurrent( float amountToRemove );
        //void RefillCurrent();
        //void DepleteCurrent();
    }
}