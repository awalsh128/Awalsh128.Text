namespace Awalsh128.Text.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;
    using MbUnit.Framework;

    [TestFixture, Parallelizable]
    [ApartmentState(System.Threading.ApartmentState.MTA)]
    public class UUDecodeStreamTests
    {
        [FixtureSetUp]
        public void TestFixtureSetUp()
        { }

        [FixtureTearDown]
        public void TestFixtureTearDown()
        { }

        [SetUp]
        public void SetUp()
        { }

        [TearDown]
        public void TearDown()
        { }

        public IEnumerable<object[]> FilePairs
        {
            get
            {
                return Directory.GetFiles(@"Files", "UUTests-*")
                    .Where(f => !f.EndsWith(".actual"))
                    .GroupBy(Path.GetFileNameWithoutExtension)
                    .Select(g => new object[] { g.ElementAt(0), g.ElementAt(1) });
            }
        }

        public IEnumerable<object[]> FilePairAndBufferSizeTuples
        {
            get
            {
                return
                    Enumerable.Range(0, 3)
                        .Select(i => (i * 3) + 1)
                        .SelectMany(
                            i => this.FilePairs.SelectMany(p => new[]
                            {
                                new[] { p[0], p[1], 1 << i }, 
                                new[] { p[0], p[1], (1 << i) - 1 }
                            }));
            }
        }

        [Test]
        [Factory("FilePairAndBufferSizeTuples")]
        public void DecodeFile(string decodedFilePath, string encodedFilePath, int bufferSize)
        {
            var hasher = MD5.Create();
            using (FileStream encodedFileStream = File.OpenRead(encodedFilePath))
            {
                using (var decodeStream = new UUDecodeStream(encodedFileStream))
                {
                    using (var unbuffered = new MemoryStream())
                    {
                        using (var decodedFileStream = new BufferedStream(unbuffered, bufferSize))
                        {
                            decodeStream.CopyTo(decodedFileStream);
                        }
                        byte[] actualDecoded = unbuffered.ToArray();
                        File.WriteAllBytes(decodedFilePath + ".actual", actualDecoded);
                        byte[] expectedDecoded = Encoding.ASCII.GetBytes(File.ReadAllText(decodedFilePath));
                        Assert.AreEqual(expectedDecoded.Length, actualDecoded.Length);

                        string actualHash = BitConverter.ToString(hasher.ComputeHash(actualDecoded));
                        string expectedHash = BitConverter.ToString(hasher.ComputeHash(expectedDecoded));
                        if (actualHash != expectedHash)
                        {
                            for (int i = 0; i < expectedDecoded.Length; i++)
                            {
                                Assert.AreEqual(expectedDecoded[i], actualDecoded[i], "Byte values must match at postiion {0}.", i);
                            }
                        }
                    }
                }
            }
        }

        [Test]
        public void Benchmark()
        {
            var watch = Stopwatch.StartNew();
            const string encodedFilePath = @"Files\UUTests-LargeWithUnixLineEnding.uue";
            using (FileStream encodedFileStream = File.OpenRead(encodedFilePath))
            {
                using (var decodeStream = new UUDecodeStream(encodedFileStream))
                {
                    using (var decodedStream = new MemoryStream())
                    {
                        decodeStream.CopyTo(decodedStream);
                    }
                }
            }
            watch.Stop();
            Assert.LessThan(watch.ElapsedMilliseconds, 500);
        }
    }
}