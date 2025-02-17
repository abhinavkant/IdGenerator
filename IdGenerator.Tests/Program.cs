using System.Collections.Concurrent;
using System.Diagnostics;
using IdGenerator;

int threadCount = 50;  // Number of parallel threads
int idCountPerThread = 1000000; // Number of IDs per thread
var generator = new SnowflakeIdGenerator(machineId: 1, dataCenterId: 1);

var idSet = new ConcurrentDictionary<long, bool>(); // Store IDs to check for duplicates
int duplicateCount = 0;
var stopwatch = Stopwatch.StartNew();

Parallel.For(0, threadCount, _ =>
{
    for (int i = 0; i < idCountPerThread; i++)
    {
        long id = generator.NextId();

        // If ID already exists, it's a collision
        if (!idSet.TryAdd(id, true))
        {
            Console.WriteLine($"⚠️ Duplicate ID detected: {id}");
            lock (idSet)
            {
                duplicateCount++;
            }
        }
    }
});

stopwatch.Stop();
long totalGenerated = threadCount * idCountPerThread;

Console.WriteLine($"\n📌 Test Summary:");
Console.WriteLine($"✅ Total IDs Generated: {totalGenerated}");
Console.WriteLine($"❌ Duplicates Found: {duplicateCount}");
Console.WriteLine($"⏱️ Execution Time: {stopwatch.ElapsedMilliseconds} ms");
Console.WriteLine($"⚡ Speed: {totalGenerated / (stopwatch.ElapsedMilliseconds / 1000.0)} IDs/sec");
