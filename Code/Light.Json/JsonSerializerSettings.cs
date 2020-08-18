using System.Collections.Generic;
using Light.GuardClauses;
using Light.Json.Buffers;
using Light.Json.Contracts;

namespace Light.Json
{
    public class JsonSerializerSettings
    {
        private IContractProvider _contractProvider;
        private IBufferProvider<char> _utf16BufferProvider;
        private IBufferProvider<byte> _utf8BufferProvider;

        public JsonSerializerSettings()
        {
            _contractProvider = new DynamicContractProvider();
            var increaseBufferSizeStrategy = new DoubleArraySizeStrategy();
            _utf16BufferProvider = new ArrayPoolCharBufferProvider(increaseBufferSizeStrategy: increaseBufferSizeStrategy);
            _utf8BufferProvider = new ArrayPoolByteBufferProvider(increaseBufferSizeStrategy: increaseBufferSizeStrategy);
        }

        public JsonSerializerSettings(IContractProvider contractProvider,
                                      IBufferProvider<char> utf16BufferProvider,
                                      IBufferProvider<byte> utf8BufferProvider)
        {
            _contractProvider = contractProvider.MustNotBeNull(nameof(contractProvider));
            _utf16BufferProvider = utf16BufferProvider.MustNotBeNull(nameof(utf16BufferProvider));
            _utf8BufferProvider = utf8BufferProvider.MustNotBeNull(nameof(utf8BufferProvider));
        }

        public IContractProvider ContractProvider
        {
            get => _contractProvider;
            set => _contractProvider = value.MustNotBeNull();
        }

        public IBufferProvider<char> Utf16BufferProvider
        {
            get => _utf16BufferProvider;
            set => _utf16BufferProvider = value.MustNotBeNull();
        }

        public IBufferProvider<byte> Utf8BufferProvider
        {
            get => _utf8BufferProvider;
            set => _utf8BufferProvider = value.MustNotBeNull();
        }
    }
}