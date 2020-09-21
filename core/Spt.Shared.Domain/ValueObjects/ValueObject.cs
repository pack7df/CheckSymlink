using Spt.Shared.Core.Services;
using Spt.Shared.Domain.Events;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace Spt.Shared.Domain.ValueObjects
{
    [DataContract]
    public abstract class ValueObject
    {
        private static readonly ConcurrentDictionary<Type, IReadOnlyCollection<PropertyInfo>> TypeProperties = new ConcurrentDictionary<Type, IReadOnlyCollection<PropertyInfo>>();

        [NotMapped]
        [IgnoreDataMember]
        private readonly List<DomainEvent> _events = new List<DomainEvent>();

        protected Func<string, string> t { get; set; }

        public ValueObject()
        {
            var textTranslator = Core.ApplicationContext.ServiceProvider.GetService<ITextTranslator>();
            t = textTranslator == null ? (Func<string, string>)(o => o) : textTranslator.Translate;
        }

        protected void RaiseEvent(DomainEvent @event)
        {
            _events.Add(@event);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj)) return true;
            if (ReferenceEquals(null, obj)) return false;

            var type1 = GetType();
            var type2 = obj.GetType();
            if (type1 != type2 && !(type1.Name == $"{type2.Name}Proxy" || type2.Name == $"{type1.Name}Proxy")) return false;

            var other = obj as ValueObject;
            return other != null && GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return GetEqualityComponents().Aggregate(17, (current, obj) => current * 23 + (obj?.GetHashCode() ?? 0));
            }
        }

        public static bool operator ==(ValueObject left, ValueObject right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ValueObject left, ValueObject right)
        {
            return !Equals(left, right);
        }

        //public override string ToString()
        //{
        //    return $"{{{string.Join(", ", GetProperties().Select(f => $"{f.Name}: {f.GetValue(this)}"))}}}";
        //}

        protected virtual IEnumerable<object> GetEqualityComponents()
        {
            return GetProperties().Select(x => x.GetValue(this));
        }

        protected virtual IEnumerable<PropertyInfo> GetProperties()
        {
            return TypeProperties.GetOrAdd(
                GetType(),
                t => t
                    .GetTypeInfo()
                    .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                    .OrderBy(p => p.Name)
                    .ToList());
        }
    }
}
