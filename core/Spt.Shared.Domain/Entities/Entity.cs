using Spt.Shared.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace Spt.Shared.Domain.Entities
{
    [DataContract]
    [Serializable]
    public abstract class Entity<TKey> : ValueObject
    {
        [NotMapped]
        [IgnoreDataMember]
        public bool IsKeySet { get; protected set; } = false;

        [DataMember]
        [Key]
        public TKey Id { get; protected set; }

        public Entity(TKey id)
        {
            IsKeySet = true;
            Id = id;
        }

        public Entity()
        {

        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Id;
        }
    }
}
