using MasterLimitTest;
using System;
using Xunit;

namespace CustomSetTest
{
    public class UnitTest1
    {
        private static readonly CustomSetFactory<string> factory1 = new();

        private static readonly CustomSetFactory<string> factory2 = new();

        private static readonly CustomSet<string> firstSetOf1fromFactory1 = MakeSet(factory1, "1");

        private static readonly CustomSet<string> secondSetOf1FromFactory1 = MakeSet(factory1, "1");

        private static readonly CustomSet<string> setOf2FromFactory1 = MakeSet(factory1, "2");

        private static readonly CustomSet<string> setOf1FromFactory2 = MakeSet(factory2, "1");

        private static CustomSet<string> MakeSet(CustomSetFactory<string> factory, string v)
        {
            var set = factory.NewSet();
            set.Add(v);
            return set.ToCustomSet();
        }

        [Fact]
        public void Test1()
        {
            Assert.True(firstSetOf1fromFactory1.Equals(secondSetOf1FromFactory1));
            Assert.Equal(firstSetOf1fromFactory1.GetHashCode(), secondSetOf1FromFactory1.GetHashCode());
        }

        [Fact]
        public void Test2()
        {
            Assert.False(firstSetOf1fromFactory1.Equals(setOf2FromFactory1));
            Assert.NotEqual(firstSetOf1fromFactory1.GetHashCode(), setOf2FromFactory1.GetHashCode());
        }

        [Fact]
        public void Test3()
        {
            Assert.False(firstSetOf1fromFactory1.Equals(setOf1FromFactory2));
            Assert.NotEqual(firstSetOf1fromFactory1.GetHashCode(), setOf1FromFactory2.GetHashCode());
        }
    }
}
