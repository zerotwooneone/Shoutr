using Microsoft.Extensions.Caching.Memory;
using Moq;

namespace Library.Tests.Extensions.Caching.Memory
{
    public static class MockMemoryCacheExtensions
    {
        public static void SetupGetOrCreate<TKey,TValue>(this Mock<IMemoryCache> memoryCache,
            Mock<ICacheEntry> cacheEntry,
            TValue existingCacheValue = null,
            bool? tryGetResult = null) where TValue: class
        {
            var tgResult = tryGetResult ?? existingCacheValue != null;
            var existingObject = (object) existingCacheValue;
            memoryCache
                .Setup(mc => mc.TryGetValue((object)It.IsAny<TKey>(), out existingObject)) 
                .Returns(tgResult);           
            if (!tgResult)
            {
                 memoryCache
                    .Setup(mc => mc.CreateEntry(It.IsAny<TKey>()))
                    .Returns(cacheEntry.Object);
                cacheEntry
                    .SetupSet(ce=>ce.Value = It.IsAny<TValue>())
                    .Verifiable();
                cacheEntry
                    .Setup(ce=>ce.Dispose())
                    .Verifiable();
            }
        }
    }
}