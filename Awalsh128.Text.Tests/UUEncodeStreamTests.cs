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
    public class UUEncodeStreamTests
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
        public void EncodeFile(string decodedFilePath, string encodedFilePath, int bufferSize)
        {
            var hasher = MD5.Create();
            bool unixLineEnding = encodedFilePath.Contains("UnixLineEnding");
            using (FileStream decodedFileStream = File.OpenRead(decodedFilePath))
            {
                using (var unbuffered = new MemoryStream())
                {
                    using (var encodedStream = new BufferedStream(unbuffered, bufferSize))
                    {
                        using (var encodeStream = new UUEncodeStream(encodedStream, unixLineEnding))
                        {
                            decodedFileStream.CopyTo(encodeStream);
                        }
                    }
                    byte[] actualEncoded = unbuffered.ToArray();
                    File.WriteAllBytes(encodedFilePath + ".actual", actualEncoded);
                    byte[] expectedEncoded = Encoding.ASCII.GetBytes(File.ReadAllText(encodedFilePath));

                    string actualHash = BitConverter.ToString(hasher.ComputeHash(actualEncoded));
                    string expectedHash = BitConverter.ToString(hasher.ComputeHash(expectedEncoded));
                    if (actualHash != expectedHash)
                    {
                        for (int i = 0; i < expectedEncoded.Length; i++)
                        {
                            Assert.AreEqual(expectedEncoded[i], actualEncoded[i], "Byte values must match at postion {0}.", i);
                        }
                    }
                }
            }
        }

        [Test]
        public void Benchmark()
        {
            var watch = Stopwatch.StartNew();
            const string decodedFilePath = @"Files\UUTests-LargeWithUnixLineEnding.txt";
            using (FileStream decodedFileStream = File.OpenRead(decodedFilePath))
            {
                using (var encodeStream = new UUEncodeStream(new MemoryStream(), true))
                {
                    decodedFileStream.CopyTo(encodeStream);
                }
            }
            watch.Stop();
            Assert.LessThan(watch.ElapsedMilliseconds, 500);
        }
    }
}