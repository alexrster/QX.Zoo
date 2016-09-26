using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading;

namespace QX.Zoo.Runtime.Toys
{
    public sealed class SequentialGuidGenerator : IGuidGenerator
    {
        private static readonly ConcurrentDictionary<Type, SequentialGuidGenerator> TypeCodes = new ConcurrentDictionary<Type, SequentialGuidGenerator>();
        private static readonly System.Security.Cryptography.RandomNumberGenerator RandomProvider = System.Security.Cryptography.RandomNumberGenerator.Create();

        private long _seqNumber;

        public short TypeCode { get; private set; }

        private SequentialGuidGenerator(short typeCode, long seqNumber)
        {
            TypeCode = typeCode;
            _seqNumber = seqNumber;
        }

        public Guid NewGuid()
        {
            var seqNumber = Interlocked.Increment(ref _seqNumber); 
            var ticks = DateTime.UtcNow.Ticks;
            byte[] d8 = new byte[8];

            if (seqNumber >> 16 > 0)
            {
                while (seqNumber != Interlocked.CompareExchange(ref _seqNumber, seqNumber >> 16, seqNumber))
                {
                    seqNumber = Interlocked.Increment(ref _seqNumber);
                }
            }

            RandomProvider.GetBytes(d8);

            d8[0] = (byte)((1 << 6) | ((byte) (seqNumber >> 11)));
            d8[1] = (byte)seqNumber;

            return new Guid(
                (Int32)(ticks >> 40), 
                (Int16)(ticks >> 16),
                (Int16)((Int16)(TypeCode << 8) | (Int16)(ticks >> 8)),
                d8
                );
        }

        public static Guid NewGuid<T>(Func<byte> byteCodeFunc = null, Func<UInt16> initSeqNumberFunc = null)
        {
            return GetOrCreateGenerator<T>(byteCodeFunc, initSeqNumberFunc).NewGuid();
        }

        public static IGuidGenerator GetOrCreateGenerator<T>(Func<byte> byteCodeFunc = null, Func<UInt16> initSeqNumberFunc = null)
        {
            return GetOrCreateGenerator(typeof(T), byteCodeFunc, initSeqNumberFunc);
        }

        public static IGuidGenerator GetOrCreateGenerator(Type type, Func<byte> byteCodeFunc = null, Func<UInt16> initSeqNumberFunc = null)
        {
            return TypeCodes.GetOrAdd(type,
                t => new SequentialGuidGenerator(
                    (byteCodeFunc ?? (() => GenerateTypeByteCode(t)))(),
                    (initSeqNumberFunc ?? (() => 0))())
                );
        }

        private static byte GenerateTypeByteCode(Type type)
        {
            return Encoding.UTF8.GetBytes(type.FullName).Aggregate((x, b) => (byte)(x + b));
        }
    }
}
