using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarcodeLaminationPrint
{
    public abstract class Entity
    {
        /// <summary>
        /// Gets or sets the unique identifier of the entity.
        /// </summary>
        public Guid Id { get; protected set; }

        public override bool Equals(object? obj)
        {
            if (obj is null || this.GetType() != obj.GetType())
                return false;

            if (ReferenceEquals(this, obj))
                return true;

            var other = (Entity)obj;
            if (this.Id.Equals(default) || other.Id.Equals(default))
                return false;

            return this.Id.Equals(other.Id);
        }

        public override int GetHashCode()
        {
            if (this.Id.Equals(default))
                return base.GetHashCode();

            return this.Id.GetHashCode() ^ 31;
        }
    }
}
