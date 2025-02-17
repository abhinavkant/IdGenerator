using IdGenerator;

var generator = new SnowflakeIdGenerator(machineId: 1, dataCenterId: 1);
long uniqueId = generator.NextId();
Console.WriteLine($"Generated Snowflake ID: {uniqueId}");