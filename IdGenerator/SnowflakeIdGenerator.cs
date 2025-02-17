namespace IdGenerator
{
    public class SnowflakeIdGenerator
    {
        private const long Twepoch = 1288834974657L; // Custom epoch timestamp
        private const int MachineIdBits = 5;
        private const int DataCenterIdBits = 5;
        private const int SequenceBits = 12;

        private const long MaxMachineId = -1L ^ (-1L << MachineIdBits);
        private const long MaxDataCenterId = -1L ^ (-1L << DataCenterIdBits);
        private const long MaxSequence = -1L ^ (-1L << SequenceBits);

        private const int MachineIdShift = SequenceBits;
        private const int DataCenterIdShift = SequenceBits + MachineIdBits;
        private const int TimestampLeftShift = SequenceBits + MachineIdBits + DataCenterIdBits;

        private readonly long _machineId;
        private readonly long _dataCenterId;
        private long _lastTimestamp = -1L;
        private long _sequence = 0L;

        private readonly object _lock = new();

        public SnowflakeIdGenerator(long machineId, long dataCenterId)
        {
            if (machineId > MaxMachineId || machineId < 0)
                throw new ArgumentException($"Machine ID must be between 0 and {MaxMachineId}");

            if (dataCenterId > MaxDataCenterId || dataCenterId < 0)
                throw new ArgumentException($"Datacenter ID must be between 0 and {MaxDataCenterId}");

            _machineId = machineId;
            _dataCenterId = dataCenterId;
        }

        public long NextId()
        {
            lock (_lock)
            {
                long timestamp = GetCurrentTimestamp();

                if (timestamp < _lastTimestamp)
                {
                    throw new InvalidOperationException("Clock moved backwards. Refusing to generate ID.");
                }

                if (timestamp == _lastTimestamp)
                {
                    _sequence = (_sequence + 1) & MaxSequence;
                    if (_sequence == 0) timestamp = WaitForNextMillis(_lastTimestamp);
                }
                else
                {
                    _sequence = 0;
                }

                _lastTimestamp = timestamp;

                return ((timestamp - Twepoch) << TimestampLeftShift)
                       | (_dataCenterId << DataCenterIdShift)
                       | (_machineId << MachineIdShift)
                       | _sequence;
            }
        }

        private long GetCurrentTimestamp()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }

        private long WaitForNextMillis(long lastTimestamp)
        {
            long timestamp = GetCurrentTimestamp();
            while (timestamp <= lastTimestamp)
            {
                timestamp = GetCurrentTimestamp();
            }
            return timestamp;
        }
    }
}