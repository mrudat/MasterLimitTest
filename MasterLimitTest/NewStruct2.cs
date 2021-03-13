using Mutagen.Bethesda;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace MasterLimitTest
{
    public partial class Program
    {
        internal class NewStruct2
        {
            public int recordCount;

            public readonly HashSet<CustomSet<ModKey>> masterSets;

            public NewStruct2(int recordCount, HashSet<CustomSet<ModKey>> masterSets)
            {
                this.recordCount = recordCount;
                this.masterSets = masterSets;
            }

            public override bool Equals(object? obj)
            {
                return obj is NewStruct2 other &&
                       recordCount == other.recordCount &&
                       EqualityComparer<HashSet<CustomSet<ModKey>>>.Default.Equals(masterSets, other.masterSets);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(recordCount, masterSets);
            }

            public void Deconstruct(out int recordCount, out HashSet<CustomSet<ModKey>> masterSets)
            {
                recordCount = this.recordCount;
                masterSets = this.masterSets;
            }

            public static implicit operator (int recordCount, HashSet<CustomSet<ModKey>> masterSets)(NewStruct2 value)
            {
                return (value.recordCount, value.masterSets);
            }

            public static implicit operator NewStruct2((int recordCount, HashSet<CustomSet<ModKey>> masterSets) value)
            {
                return new NewStruct2(value.recordCount, value.masterSets);
            }
        }
    }
}