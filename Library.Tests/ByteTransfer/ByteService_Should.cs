using System;
using System.Numerics;
using Library.ByteTransfer;
using Xunit;

namespace Library.Tests.ByteTransfer
{
    public class ByteService_Should
    {
        private readonly ByteService _byteService;

        public ByteService_Should()
        {
            _byteService = new ByteService();
        }

        [Fact]
        public void GetBytesBigInt_ReturnsOneByte_GivenOne()
        {
            //act
            const int expected = 1;
            var actual = _byteService.GetBytes(new BigInteger(1))
                .Length;

            //assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GetBytesBigInt_ReturnsOne_GivenOne()
        {
            //act
            const byte expected = 1;
            var actual = _byteService.GetBytes(new BigInteger(1))[0];

            //assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GetBytesBigInt_ReturnsTwoBytes_Given256()
        {
            //act
            const int expected = 2;
            var actual = _byteService.GetBytes(new BigInteger(256))
                .Length;

            //assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GetBytesGuid_Returns16Bytes()
        {
            //act
            const int expected = 16;
            var actual = _byteService.GetBytes(Guid.Empty)
                .Length;

            //assert
            Assert.Equal(expected, actual);
        }
    }
}