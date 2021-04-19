using System;

namespace Macros.Autowire
{
    public readonly struct Scope
    {
        public bool HasValue { get; }
        public long Id { get; }
        public Scope(long id)
        {
            Id = id;
            HasValue = true;
        }

        public override bool Equals(object obj)
        {
            return obj is Scope scope &&
                   Id == scope.Id;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id);
        }

        public static void Destroy(long id)
        {
            Storage.Current.TryRemoveScope(new Scope(id));
        }
        public static void Destroy(Scope scope)
        {
            Storage.Current.TryRemoveScope(scope);
        }
    }
}